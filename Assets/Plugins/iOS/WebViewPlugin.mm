#import <UIKit/UIKit.h>
#import <WebKit/WebKit.h>

@interface WebViewPlugin : NSObject<WKNavigationDelegate>
@property (nonatomic, strong) WKWebView *webView;
@property (nonatomic, strong) UIView *containerView;
@property (nonatomic, copy) NSString *gameObjectName;
@end

@implementation WebViewPlugin

static WebViewPlugin *sharedInstance = nil;

+ (WebViewPlugin *)sharedInstance {
    if (sharedInstance == nil) {
        sharedInstance = [[WebViewPlugin alloc] init];
    }
    return sharedInstance;
}

- (void)initWebView:(NSString *)gameObjectName x:(int)x y:(int)y width:(int)width height:(int)height {
    self.gameObjectName = gameObjectName;
    
    dispatch_async(dispatch_get_main_queue(), ^{
        // Create container view
        self.containerView = [[UIView alloc] initWithFrame:CGRectMake(x, y, width, height)];
        self.containerView.backgroundColor = [UIColor whiteColor];
        
        // Create WKWebView
        WKWebViewConfiguration *config = [[WKWebViewConfiguration alloc] init];
        config.allowsInlineMediaPlayback = YES;
        config.mediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypeNone;
        
        self.webView = [[WKWebView alloc] initWithFrame:self.containerView.bounds configuration:config];
        self.webView.navigationDelegate = self;
        self.webView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
        
        [self.containerView addSubview:self.webView];
        
        // Add to main window
        UIWindow *window = [UIApplication sharedApplication].keyWindow;
        if (window) {
            [window addSubview:self.containerView];
        }
    });
}

- (void)loadURL:(NSString *)url {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.webView) {
            NSURL *nsurl = [NSURL URLWithString:url];
            NSURLRequest *request = [NSURLRequest requestWithURL:nsurl];
            [self.webView loadRequest:request];
        }
    });
}

- (void)setVisibility:(BOOL)visible {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.containerView) {
            self.containerView.hidden = !visible;
        }
    });
}

- (void)destroy {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.webView) {
            [self.webView removeFromSuperview];
            self.webView = nil;
        }
        if (self.containerView) {
            [self.containerView removeFromSuperview];
            self.containerView = nil;
        }
    });
}

#pragma mark - WKNavigationDelegate

- (void)webView:(WKWebView *)webView didStartProvisionalNavigation:(WKNavigation *)navigation {
    // Page started loading
}

- (void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation {
    // Page finished loading
    if (self.gameObjectName) {
        NSString *url = webView.URL.absoluteString;
        UnitySendMessage([self.gameObjectName UTF8String], "OnPageLoadedCallback", [url UTF8String]);
    }
}

- (void)webView:(WKWebView *)webView didFailNavigation:(WKNavigation *)navigation withError:(NSError *)error {
    // Navigation failed
    if (self.gameObjectName) {
        NSString *errorMessage = [NSString stringWithFormat:@"Error loading page: %@", error.localizedDescription];
        UnitySendMessage([self.gameObjectName UTF8String], "OnErrorCallback", [errorMessage UTF8String]);
    }
}

- (void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler {
    // Handle URL changes
    if (self.gameObjectName) {
        NSString *url = navigationAction.request.URL.absoluteString;
        UnitySendMessage([self.gameObjectName UTF8String], "OnUrlChangedCallback", [url UTF8String]);
    }
    
    decisionHandler(WKNavigationActionPolicyAllow);
}

@end

extern "C" {
    void _WebView_Init(const char *gameObjectName, int x, int y, int width, int height) {
        NSString *objName = [NSString stringWithUTF8String:gameObjectName];
        [[WebViewPlugin sharedInstance] initWebView:objName x:x y:y width:width height:height];
    }
    
    void _WebView_LoadURL(const char *url) {
        NSString *urlString = [NSString stringWithUTF8String:url];
        [[WebViewPlugin sharedInstance] loadURL:urlString];
    }
    
    void _WebView_SetVisibility(bool visible) {
        [[WebViewPlugin sharedInstance] setVisibility:visible];
    }
    
    void _WebView_Destroy() {
        [[WebViewPlugin sharedInstance] destroy];
    }
}

