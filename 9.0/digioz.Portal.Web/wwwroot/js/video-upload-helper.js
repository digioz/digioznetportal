/**
 * Video Upload Helper
 * Wrapper around ChunkedFileUploader specifically for video file uploads
 * Integrates with video upload forms
 */

class VideoUploadHelper {
    constructor(options) {
        this.formId = options.formId;
        this.videoInputId = options.videoInputId;
        this.thumbnailInputId = options.thumbnailInputId;
        this.submitButtonId = options.submitButtonId;
        this.progressContainerId = options.progressContainerId || 'upload-progress';
        this.chunkSizeInMB = options.chunkSizeInMB || 20;
        this.onSuccess = options.onSuccess || function() {};
        this.onError = options.onError || function() {};
        
        this.form = null;
        this.videoInput = null;
        this.thumbnailInput = null;
        this.submitButton = null;
        this.originalSubmitText = '';
        this.uploader = null;
        this.assembledVideoPath = null;
        
        this.init();
    }

    init() {
        this.form = document.getElementById(this.formId);
        this.videoInput = document.getElementById(this.videoInputId);
        this.thumbnailInput = document.getElementById(this.thumbnailInputId);
        this.submitButton = document.getElementById(this.submitButtonId);
        
        if (!this.form || !this.videoInput || !this.submitButton) {
            console.error('Required elements not found');
            return;
        }

        this.originalSubmitText = this.submitButton.textContent;
        
        // Intercept form submission
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));
    }

    async handleSubmit(e) {
        e.preventDefault();
        
        const videoFile = this.videoInput.files[0];
        
        if (!videoFile) {
            alert('Please select a video file');
            return;
        }

        // Check if file is large enough to require chunking (> 50MB)
        const fileSizeInMB = videoFile.size / (1024 * 1024);
        const chunkThresholdMB = 50;
        
        if (fileSizeInMB <= chunkThresholdMB) {
            // File is small enough, use normal form submission
            console.log(`File size (${fileSizeInMB.toFixed(2)}MB) below chunking threshold, using standard upload`);
            this.form.submit();
            return;
        }

        console.log(`File size (${fileSizeInMB.toFixed(2)}MB) requires chunked upload`);
        
        // IMPORTANT: Capture form data BEFORE disabling anything
        // Store the current form state for later submission
        this.capturedFormData = new FormData(this.form);
        
        // Disable form during upload
        this.disableForm();
        
        // Show progress UI
        this.showProgress();
        
        try {
            // Upload video using chunked upload
            const chunkSizeBytes = this.chunkSizeInMB * 1024 * 1024;
            this.uploader = new ChunkedFileUploader({
                fileInput: this.videoInput,
                uploadUrl: '/API/ChunkedUpload',
                chunkSize: chunkSizeBytes,
                maxRetries: 5,        // Retry failed chunks up to 5 times
                retryDelay: 2000,     // 2 second initial retry delay
                chunkDelay: 1500,     // 1.5 second delay between chunks to prevent connection issues
                onProgress: (percent, current, total) => this.updateProgress(percent, current, total),
                onComplete: (result) => this.handleUploadComplete(result),
                onError: (error) => this.handleUploadError(error)
            });

            const result = await this.uploader.upload(videoFile, {
                fileType: 'video'
            });
            
            if (result) {
                // Store the assembled video path and submit form
                this.assembledVideoPath = result.relativePath;
                await this.submitFormWithAssembledVideo();
            }
            
        } catch (error) {
            this.handleUploadError(error.message);
        }
    }

    async submitFormWithAssembledVideo() {
        console.log('Submitting form with assembled video path:', this.assembledVideoPath);
        
        // Update progress
        const progressDetail = document.getElementById('progress-detail');
        if (progressDetail) {
            progressDetail.textContent = 'Saving video...';
        }
        
        // Use the form data we captured BEFORE disabling the form
        const formData = this.capturedFormData;
        
        // Add the assembled video path
        formData.append('AssembledVideoPath', this.assembledVideoPath);
        
        // Remove the video file from FormData since we're using the assembled path
        formData.delete('VideoFile');
        
        console.log('Submitting form...');
        
        // Log all form data for debugging
        console.log('Form data being submitted:');
        for (let pair of formData.entries()) {
            if (pair[1] instanceof File) {
                console.log('FormData:', pair[0], '=', '[File]', pair[1].name, pair[1].size, 'bytes');
            } else {
                console.log('FormData:', pair[0], '=', pair[1]);
            }
        }
        
        // Submit using fetch to have better control
        try {
            const response = await fetch(this.form.action || window.location.href, {
                method: 'POST',
                body: formData,
                // Don't set Content-Type header - let browser set it with boundary
            });
            
            console.log('Response status:', response.status);
            
            if (response.ok) {
                // Successful submission, check if it's a redirect or page reload
                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('text/html')) {
                    // Server returned HTML, likely the result page
                    const html = await response.text();
                    document.open();
                    document.write(html);
                    document.close();
                } else {
                    // Just reload the page
                    window.location.reload();
                }
            } else {
                console.error('Form submission failed:', response.status, response.statusText);
                const text = await response.text();
                console.error('Response body:', text);
                
                // Try to extract error message from response
                const parser = new DOMParser();
                const doc = parser.parseFromString(text, 'text/html');
                const errorElements = doc.querySelectorAll('.validation-summary-errors li, .text-danger');
                let errorMessage = 'Error saving video. Please check the console for details.';
                
                if (errorElements.length > 0) {
                    errorMessage = Array.from(errorElements).map(el => el.textContent).join('\n');
                }
                
                alert(errorMessage);
                this.enableForm();
                this.hideProgress();
            }
        } catch (error) {
            console.error('Form submission error:', error);
            alert('Error submitting form: ' + error.message);
            this.enableForm();
            this.hideProgress();
        }
    }

    showProgress() {
        let progressHtml = `
            <div id="${this.progressContainerId}" class="alert alert-info mt-3">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <strong>Uploading video...</strong>
                    <span id="progress-percent">0%</span>
                </div>
                <div class="progress" style="height: 25px;">
                    <div id="progress-bar" class="progress-bar progress-bar-striped progress-bar-animated" 
                         role="progressbar" style="width: 0%"></div>
                </div>
                <small id="progress-detail" class="text-muted d-block mt-2">Preparing upload...</small>
            </div>
        `;
        
        // Insert progress UI before the submit button
        const progressContainer = document.createElement('div');
        progressContainer.innerHTML = progressHtml;
        this.submitButton.parentNode.insertBefore(progressContainer.firstElementChild, this.submitButton);
    }

    updateProgress(percent, current, total) {
        const progressBar = document.getElementById('progress-bar');
        const progressPercent = document.getElementById('progress-percent');
        const progressDetail = document.getElementById('progress-detail');
        
        if (progressBar) {
            progressBar.style.width = percent + '%';
            progressBar.setAttribute('aria-valuenow', percent);
        }
        
        if (progressPercent) {
            progressPercent.textContent = percent + '%';
        }
        
        if (progressDetail) {
            progressDetail.textContent = `Uploading chunk ${current} of ${total}...`;
        }
    }

    handleUploadComplete(result) {
        console.log('Upload complete:', result);
        const progressDetail = document.getElementById('progress-detail');
        if (progressDetail) {
            progressDetail.textContent = 'Finalizing upload...';
        }
    }

    handleUploadError(error) {
        console.error('Upload error:', error);
        this.hideProgress();
        this.enableForm();
        alert('Error uploading video: ' + error);
        this.onError(error);
    }

    hideProgress() {
        const progressContainer = document.getElementById(this.progressContainerId);
        if (progressContainer) {
            progressContainer.remove();
        }
    }

    disableForm() {
        this.submitButton.disabled = true;
        this.submitButton.textContent = 'Uploading...';
        if (this.videoInput) this.videoInput.disabled = true;
        if (this.thumbnailInput) this.thumbnailInput.disabled = true;
        
        // Disable all other form inputs
        const inputs = this.form.querySelectorAll('input, select, textarea, button');
        inputs.forEach(input => {
            if (input.id !== this.submitButtonId) {
                input.disabled = true;
            }
        });
    }

    enableForm() {
        this.submitButton.disabled = false;
        this.submitButton.textContent = this.originalSubmitText;
        if (this.videoInput) this.videoInput.disabled = false;
        if (this.thumbnailInput) this.thumbnailInput.disabled = false;
        
        // Enable all other form inputs
        const inputs = this.form.querySelectorAll('input, select, textarea, button');
        inputs.forEach(input => input.disabled = false);
    }
}

// Make available globally
window.VideoUploadHelper = VideoUploadHelper;
