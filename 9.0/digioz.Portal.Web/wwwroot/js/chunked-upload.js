/**
 * Chunked File Upload Library
 * Handles large file uploads by breaking them into smaller chunks
 * to work within Cloudflare's 100MB upload limit
 */

class ChunkedFileUploader {
    constructor(options) {
        this.fileInput = options.fileInput;
        this.uploadUrl = options.uploadUrl || '/API/ChunkedUpload';
        this.chunkSize = options.chunkSize || (20 * 1024 * 1024); // Default 20MB in bytes
        this.onProgress = options.onProgress || function() {};
        this.onComplete = options.onComplete || function() {};
        this.onError = options.onError || function() {};
        this.maxRetries = options.maxRetries || 3; // Retry failed chunks
        this.retryDelay = options.retryDelay || 1000; // 1 second delay between retries
        this.chunkDelay = options.chunkDelay || 200; // Small delay between chunks to prevent overwhelming
        this.file = null;
        this.uploadId = null;
        this.currentChunk = 0;
        this.totalChunks = 0;
        this.aborted = false;
    }

    /**
     * Start the chunked upload process
     * @param {File} file - The file to upload
     * @param {Object} metadata - Additional metadata to send with the upload
     */
    async upload(file, metadata = {}) {
        if (!file) {
            this.onError('No file provided');
            return null;
        }

        this.file = file;
        this.uploadId = this.generateUploadId();
        this.currentChunk = 0;
        this.totalChunks = Math.ceil(file.size / this.chunkSize);
        this.aborted = false;

        console.log(`Starting chunked upload: ${file.name} (${this.formatFileSize(file.size)})`);
        console.log(`Total chunks: ${this.totalChunks}, Chunk size: ${this.formatFileSize(this.chunkSize)}`);
        console.log(`Max retries per chunk: ${this.maxRetries}, Delay between chunks: ${this.chunkDelay}ms`);

        try {
            // Upload each chunk sequentially
            for (let chunkIndex = 0; chunkIndex < this.totalChunks; chunkIndex++) {
                if (this.aborted) {
                    console.log('Upload aborted by user');
                    return null;
                }

                this.currentChunk = chunkIndex;
                const start = chunkIndex * this.chunkSize;
                const end = Math.min(start + this.chunkSize, file.size);
                const chunk = file.slice(start, end);

                // Upload with retry logic
                await this.uploadChunkWithRetry(chunk, chunkIndex, metadata);

                const progress = Math.round(((chunkIndex + 1) / this.totalChunks) * 100);
                this.onProgress(progress, chunkIndex + 1, this.totalChunks);

                // Small delay between chunks to prevent overwhelming the connection
                if (chunkIndex < this.totalChunks - 1) {
                    await this.delay(this.chunkDelay);
                }
            }

            // All chunks uploaded, now assemble
            const result = await this.assembleChunksWithRetry(metadata);
            this.onComplete(result);
            return result;

        } catch (error) {
            console.error('Upload error:', error);
            this.onError(error.message || 'Upload failed');
            // Clean up chunks on error
            await this.cleanup();
            return null;
        }
    }

    /**
     * Upload a single chunk with retry logic
     */
    async uploadChunkWithRetry(chunk, chunkIndex, metadata) {
        let lastError;
        
        for (let attempt = 1; attempt <= this.maxRetries; attempt++) {
            try {
                console.log(`Uploading chunk ${chunkIndex + 1}/${this.totalChunks} (attempt ${attempt}/${this.maxRetries})`);
                await this.uploadChunk(chunk, chunkIndex, metadata);
                return; // Success, exit retry loop
            } catch (error) {
                lastError = error;
                console.warn(`Chunk ${chunkIndex + 1} upload failed (attempt ${attempt}/${this.maxRetries}):`, error.message);
                
                if (attempt < this.maxRetries) {
                    // Wait before retrying, with exponential backoff
                    const waitTime = this.retryDelay * Math.pow(2, attempt - 1);
                    console.log(`Retrying in ${waitTime}ms...`);
                    await this.delay(waitTime);
                } else {
                    // All retries exhausted
                    throw new Error(`Failed to upload chunk ${chunkIndex + 1} after ${this.maxRetries} attempts: ${error.message}`);
                }
            }
        }
        
        throw lastError;
    }

    /**
     * Upload a single chunk
     */
    async uploadChunk(chunk, chunkIndex, metadata) {
        const formData = new FormData();
        formData.append('chunk', chunk);
        formData.append('uploadId', this.uploadId);
        formData.append('chunkIndex', chunkIndex);
        formData.append('totalChunks', this.totalChunks);
        formData.append('fileName', this.file.name);
        formData.append('fileSize', this.file.size);
        
        // Add any additional metadata
        Object.keys(metadata).forEach(key => {
            formData.append(key, metadata[key]);
        });

        // Create abort controller for timeout
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 90000); // 90 second timeout per chunk (increased)

        try {
            const response = await fetch(this.uploadUrl, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Connection': 'keep-alive',
                    'Cache-Control': 'no-cache'
                },
                signal: controller.signal,
                // Keep connection alive and disable caching
                keepalive: true,
                cache: 'no-store'
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Chunk ${chunkIndex} upload failed: ${errorText}`);
            }

            const result = await response.json();
            console.log(`Chunk ${chunkIndex + 1}/${this.totalChunks} uploaded successfully`);
            return result;
        } catch (error) {
            clearTimeout(timeoutId);
            
            // Provide more specific error messages
            if (error.name === 'AbortError') {
                throw new Error(`Chunk ${chunkIndex} upload timed out after 90 seconds`);
            } else if (error.message.includes('Failed to fetch') || error.message.includes('NetworkError')) {
                throw new Error(`Network error uploading chunk ${chunkIndex}. Connection may be unstable.`);
            } else {
                throw error;
            }
        }
    }

    /**
     * Assemble all chunks into final file with retry logic
     */
    async assembleChunksWithRetry(metadata) {
        let lastError;
        
        for (let attempt = 1; attempt <= this.maxRetries; attempt++) {
            try {
                console.log(`Assembling chunks (attempt ${attempt}/${this.maxRetries})`);
                return await this.assembleChunks(metadata);
            } catch (error) {
                lastError = error;
                console.warn(`Assembly failed (attempt ${attempt}/${this.maxRetries}):`, error.message);
                
                if (attempt < this.maxRetries) {
                    const waitTime = this.retryDelay * Math.pow(2, attempt - 1);
                    console.log(`Retrying assembly in ${waitTime}ms...`);
                    await this.delay(waitTime);
                } else {
                    throw new Error(`Failed to assemble file after ${this.maxRetries} attempts: ${error.message}`);
                }
            }
        }
        
        throw lastError;
    }

    /**
     * Assemble all chunks into final file
     */
    async assembleChunks(metadata) {
        const formData = new FormData();
        formData.append('uploadId', this.uploadId);
        formData.append('fileName', this.file.name);
        formData.append('totalChunks', this.totalChunks);
        formData.append('fileSize', this.file.size);
        
        // Add any additional metadata
        Object.keys(metadata).forEach(key => {
            formData.append(key, metadata[key]);
        });

        const response = await fetch('/API/ChunkedUploadAssemble', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Assembly failed: ${errorText}`);
        }

        const result = await response.json();
        console.log('File assembled successfully:', result);
        return result;
    }

    /**
     * Clean up uploaded chunks (called on error or cancellation)
     */
    async cleanup() {
        try {
            await fetch('/API/ChunkedUploadCleanup', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    uploadId: this.uploadId
                })
            });
            console.log('Chunks cleaned up');
        } catch (error) {
            console.error('Cleanup error:', error);
        }
    }

    /**
     * Abort the current upload
     */
    abort() {
        this.aborted = true;
        this.cleanup();
    }

    /**
     * Delay helper for retry logic
     */
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    /**
     * Generate a unique upload ID
     */
    generateUploadId() {
        return 'upload_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    /**
     * Format file size for display
     */
    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }
}

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ChunkedFileUploader;
}
