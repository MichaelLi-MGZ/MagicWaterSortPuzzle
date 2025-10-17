/**
 * Unity Gateway - Communication bridge between Unity and JavaScript
 * Based on unity-web-view repository
 */

class UnityGateway {
    constructor() {
        this.isUnity = this.detectUnity();
        this.onMessage = null;
        this.messageQueue = [];
        this.initialized = false;
        
        if (this.isUnity) {
            this.initGateway();
        }
    }
    
    /**
     * Detect if running in Unity environment
     */
    detectUnity() {
        return typeof window !== 'undefined' && 
               (window.unityInstance !== undefined || 
                window.webkit !== undefined || 
                navigator.userAgent.includes('Unity'));
    }
    
    /**
     * Initialize the Unity gateway
     */
    async initGateway() {
        if (this.initialized) return;
        
        try {
            // Wait for Unity to be ready
            await this.waitForUnity();
            this.initialized = true;
            
            if (this.onMessage) {
                this.onMessage({ type: 'gateway_ready', message: 'Unity Gateway initialized' });
            }
            
            // Process any queued messages
            this.processMessageQueue();
            
        } catch (error) {
            console.error('Unity Gateway initialization failed:', error);
        }
    }
    
    /**
     * Wait for Unity to be available
     */
    waitForUnity() {
        return new Promise((resolve, reject) => {
            let attempts = 0;
            const maxAttempts = 50;
            
            const checkUnity = () => {
                attempts++;
                
                if (this.isUnityReady()) {
                    resolve();
                } else if (attempts >= maxAttempts) {
                    reject(new Error('Unity not available after maximum attempts'));
                } else {
                    setTimeout(checkUnity, 100);
                }
            };
            
            checkUnity();
        });
    }
    
    /**
     * Check if Unity is ready for communication
     */
    isUnityReady() {
        return typeof window !== 'undefined' && 
               (window.unityInstance !== undefined || 
                window.webkit !== undefined);
    }
    
    /**
     * Call Unity method with parameters
     */
    callUnity(action, ...parameters) {
        if (!this.isUnity) {
            console.warn('Not running in Unity environment');
            return Promise.resolve({ isUnity: false });
        }
        
        return new Promise((resolve, reject) => {
            try {
                const message = this.buildMessage(action, parameters);
                
                if (this.initialized) {
                    this.sendToUnity(message, resolve, reject);
                } else {
                    // Queue the message until Unity is ready
                    this.messageQueue.push({ message, resolve, reject });
                }
                
            } catch (error) {
                reject(error);
            }
        });
    }
    
    /**
     * Build message string for Unity
     */
    buildMessage(action, parameters) {
        const paramString = parameters.map(param => 
            typeof param === 'object' ? JSON.stringify(param) : String(param)
        ).join('|');
        
        return `${action}|${paramString}`;
    }
    
    /**
     * Send message to Unity
     */
    sendToUnity(message, resolve, reject) {
        try {
            if (window.unityInstance) {
                // Unity WebGL
                window.unityInstance.SendMessage('WebViewSetup', 'MessageFromWebView', message);
            } else if (window.webkit && window.webkit.messageHandlers) {
                // Unity iOS/Android
                window.webkit.messageHandlers.unityMessage.postMessage(message);
            } else {
                // Fallback - try to find Unity object
                if (window.SendMessage) {
                    window.SendMessage('WebViewSetup', 'MessageFromWebView', message);
                } else {
                    throw new Error('Unity communication not available');
                }
            }
            
            // Set up timeout for response
            setTimeout(() => {
                resolve({ success: true, message: 'Message sent to Unity' });
            }, 100);
            
        } catch (error) {
            reject(error);
        }
    }
    
    /**
     * Process queued messages
     */
    processMessageQueue() {
        while (this.messageQueue.length > 0) {
            const { message, resolve, reject } = this.messageQueue.shift();
            this.sendToUnity(message, resolve, reject);
        }
    }
    
    /**
     * Handle message from Unity
     */
    handleUnityMessage(jsonObject) {
        if (this.onMessage) {
            this.onMessage(jsonObject);
        }
    }
    
    /**
     * Send message to Unity (direct method)
     */
    sendMessage(action, ...parameters) {
        return this.callUnity(action, ...parameters);
    }
    
    /**
     * Close WebView
     */
    close() {
        return this.callUnity('close');
    }
    
    /**
     * Go back in WebView
     */
    goBack() {
        return this.callUnity('back');
    }
    
    /**
     * Load URL in WebView
     */
    loadURL(url) {
        return this.callUnity('loadURL', url);
    }
    
    /**
     * Show WebView
     */
    show() {
        return this.callUnity('show');
    }
    
    /**
     * Hide WebView
     */
    hide() {
        return this.callUnity('hide');
    }
}

// Create global instance
window.UnityGateway = new UnityGateway();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UnityGateway;
}

// Auto-initialize if in Unity environment
if (window.UnityGateway.isUnity) {
    window.UnityGateway.initGateway().catch(error => {
        console.error('Auto-initialization failed:', error);
    });
}
