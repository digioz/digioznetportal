// Get Language 
function getLanguage() {
    var lang = '';
    if (navigator.userLanguage) {
        lang = navigator.userLanguage;
    }
    else if (navigator.language) {
        lang = navigator.language;
    }
    return lang;
}

// Browser Info
function get_browser() {
    var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];

    if (/trident/i.test(M[1])) {
        tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
        return { name: 'IE', version: (tem[1] || '') };
    }
    if (M[1] === 'Chrome') {
        tem = ua.match(/\bOPR|Edge\/(\d+)/)
        if (tem != null) { return { name: 'Opera-Edge', version: tem[1] }; }
    }
    M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
    if ((tem = ua.match(/version\/(\d+)/i)) != null) { M.splice(1, 1, tem[1]); }
    return {
        name: M[0],
        version: M[1]
    };
}

// Get Screen Width
function screenWidth() {

    if (window.screen) {
        return (screen.width);
    } else {
        return (0);
    }
}

// Get Screen Height
function screenHeight() {

    if (window.screen) {
        return (screen.height);
    } else {
        return (0);
    }
}

// Generate a Session ID
function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
        s4() + '-' + s4() + s4() + s4();
}

// Store Session Info in Cookie
function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

// Set the cookie
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

// Print Results to Console Log
function printResults() {
    console.log('User Language: ' + language);
    console.log('Platform: ' + appPlatform);
    console.log('User Agent: ' + userAgent);
    console.log('Java Enabled: ' + javaEnabled);
    console.log('Browser Name: ' + browserType);
    console.log('Browser Version: ' + browserVersion);
    console.log('Screen Width: ' + screenWidth);
    console.log('Screen Height: ' + screenHeight);
    console.log('Host: ' + host);
    console.log('Host Name: ' + hostName);
    console.log('Referrer: ' + referrer);
    console.log('HREF: ' + href);
    console.log('Browser Engine Name: ' + engineName);
    console.log('Session Id: ' + sessionId);
    console.log('Session Create Date: ' + sessionCreateDate);
    console.log('OS Name: ' + osName);
}

// Make Call to Controller to Write Visit
function callAjax(url, callback) {
    var xmlhttp;
    // compatible with IE7+, Firefox, Chrome, Opera, Safari and Edge
    xmlhttp = new XMLHttpRequest();
    //xmlhttp.onreadystatechange = function () {
    //    if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
    //        callback(xmlhttp.responseText);
    //    }
    //}
    xmlhttp.open("GET", url, true);
    xmlhttp.send();
}

// Define all variables
var language = getLanguage();
var userAgent = navigator.userAgent;
var javaEnabled = navigator.javaEnabled();

// Get Browser Info
//var browser = get_browser();
//var browserVersion = browser.version;
//var browserType = browser.name;

const browserArray = bowser.getParser(window.navigator.userAgent);
var browserObj = browserArray.getBrowser();
var browserName = browserObj["name"];
var browser = browserName;
var browserVersion = browserObj["version"];
var browserType = browserObj["name"];

var parsedResult = browserArray.parsedResult;
var osName = parsedResult.os.name;
var appPlatform = parsedResult.platform.type;

var screenWidth = screenWidth();
var screenHeight = screenHeight();
var host = location.host;
var hostName = location.hostname;
var referrer = document.referrer;
var href = location.href;
var engineName = navigator.product;
var sessionId = getCookie("sessionId");
var sessionCreateDate = getCookie("sessionCreateDate");

if (sessionId == "" || sessionId == null) {
    sessionId = guid();
    setCookie("sessionId", sessionId, 1);
}

if (sessionCreateDate == "" || sessionCreateDate == null) {
    var currentdate = new Date();
    setCookie("sessionCreateDate", currentdate, 1);
}

var baseUrl = window.location.origin;
var urlTracking = baseUrl + '/Visitor/Process';

urlTracking += '?language=' + encodeURI(language) + '&appPlatform=' + encodeURI(appPlatform) + '&userAgent=' + encodeURI(userAgent)
    + '&javaEnabled=' + encodeURI(javaEnabled) + '&browserVersion=' + encodeURI(browserVersion) + '&browserType=' + encodeURI(browserType)
    + '&screenWidth=' + encodeURI(screenWidth) + '&screenHeight=' + encodeURI(screenHeight) + '&host=' + encodeURI(host)
    + '&hostName=' + encodeURI(hostName) + '&referrer=' + encodeURI(referrer) + '&href=' + encodeURI(href) + '&engineName=' + encodeURI(engineName)
    + '&sessionId=' + encodeURI(sessionId) + '&operatingSystem=' + encodeURI(osName);

if (href.indexOf('localhost') != -1) {
    printResults();
    //console.log('URL Tracking: ' + urlTracking);
}

callAjax(urlTracking, null);
