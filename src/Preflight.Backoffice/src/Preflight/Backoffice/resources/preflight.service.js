export class PreflightService {

    static serviceName = 'preflightService';

    $http;
    umbRequestHelper;

    urlBase = Umbraco.Sys.ServerVariables.Preflight.ApiPath;

    static helpText = `
        <p>If your content is too difficult for your visitors to read, you're all going to have a bad time.</p>
        <p>The readability test runs your content through the Flesch reading ease algorithm to determine text complexity.</p>
        <h5>The algorithm</h5>
        <p><code>RE = 206.835 - (1.015 x ASL) - (84.6 x ASW)</code></p>
        <p>Where <code>RE</code> is Readability Ease, <code>ASL</code> is Average Sentence Length, and <code>ASW</code> is Average Syllables per Word</p>
        <p>The result is a number between 0 and 100, where a higher score means better readability, with a score between 60 and 69 largely considered acceptable.</p>
        <h5>Readability test results</h5>
        <p>As well as the Flesch score, the readability test returns sentence length; average syllables per word; and long or complex words;</p>`;


    constructor($http, umbRequestHelper) {
        this.$http = $http;
        this.umbRequestHelper = umbRequestHelper;
    }

    request = (method, url, data) =>
        this.umbRequestHelper.resourcePromise(
            method === 'GET' ? this.$http.get(this.urlBase + url) : this.$http.post(this.urlBase + url, data),
            'Something broke'
        );

    check = id => this.request('GET', `Check/${id} `);

    checkDirty = data => this.request('POST', `CheckDirty`, data);

    getSettings = () => this.request('GET', `GetSettings`);

    getSettingValue = alias => this.request('GET', `GetSettingValue/${alias} `);

    saveSettings = (settings, tabs) => this.request('POST', `SaveSettings`, {
        settings,
        tabs
    });

    getHelpText = () => helpText;
}