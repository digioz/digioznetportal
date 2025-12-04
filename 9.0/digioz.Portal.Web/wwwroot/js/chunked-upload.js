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

                await this.uploadChunk(chunk, chunkIndex, metadata);

                const progress = Math.round(((chunkIndex + 1) / this.totalChunks) * 100);
                this.onProgress(progress, chunkIndex + 1, this.totalChunks);
            }

            // All chunks uploaded, now assemble
            const result = await this.assembleChunks(metadata);
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

        const response = await fetch(this.uploadUrl, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Chunk ${chunkIndex} upload failed: ${errorText}`);
        }

        const result = await response.json();
        console.log(`Chunk ${chunkIndex + 1}/${this.totalChunks} uploaded successfully`);
        return result;
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
