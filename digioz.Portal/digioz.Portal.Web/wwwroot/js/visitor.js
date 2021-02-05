// User Language
var language = getLanguage();

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

// Platform
var appPlatform = navigator.platform;

// User Agent
var userAgent = navigator.userAgent;

// Java Enabled
var javaEnabled = navigator.javaEnabled();

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

var browser = get_browser();
var browserVersion = browser.version;
var browserType = browser.name;

// Screen Width
var screenWidth = screenWidth();

function screenWidth() {

    if (window.screen) {
        return (screen.width);
    } else {
        return (0);
    }
}

// Screen Height
var screenHeight = screenHeight();

function screenHeight() {

    if (window.screen) {
        return (screen.height);
    } else {
        return (0);
    }
}

// Host & Host Name
var host = location.host;
var hostName = location.hostname;

// Referrer
var referrer = document.referrer;

// Get the full HREF
var href = location.href;

// Engine Name 
var engineName = navigator.product;

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

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

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
}

var baseUrl = window.location.origin;
var urlTracking = baseUrl + '/Visitor/Process';

urlTracking += '?language=' + encodeURI(language) + '&appPlatform=' + encodeURI(appPlatform) + '&userAgent=' + encodeURI(userAgent)
    + '&javaEnabled=' + encodeURI(javaEnabled) + '&browserVersion=' + encodeURI(browserVersion) + '&browserType=' + encodeURI(browserType)
    + '&screenWidth=' + encodeURI(screenWidth) + '&screenHeight=' + encodeURI(screenHeight) + '&host=' + encodeURI(host)
    + '&hostName=' + encodeURI(hostName) + '&referrer=' + encodeURI(referrer) + '&href=' + encodeURI(href) + '&engineName=' + encodeURI(engineName)
    + '&sessionId=' + encodeURI(sessionId);

if (href.indexOf('localhost') != -1) {
    printResults();
    //console.log('URL Tracking: ' + urlTracking);
}

callAjax(urlTracking, null);

function callAjax(url, callback) {
    var xmlhttp;
    // compatible with IE7+, Firefox, Chrome, Opera, Safari
    xmlhttp = new XMLHttpRequest();
    //xmlhttp.onreadystatechange = function () {
    //    if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
    //        callback(xmlhttp.responseText);
    //    }
    //}
    xmlhttp.open("GET", url, true);
    xmlhttp.send();
}

