export class PreflightService {

    static serviceName = 'preflightService';

    $http;
    umbRequestHelper;

    urlBase = Umbraco.Sys.ServerVariables.Preflight.ApiPath;

    constructor($http, umbRequestHelper) {
        this.$http = $http;
        this.umbRequestHelper = umbRequestHelper;
    }

    request = (method, url, data) =>
        this.umbRequestHelper.resourcePromise(
            method === 'GET' ? this.$http.get(this.urlBase + url) : this.$http.post(this.urlBase + url, data),
            'Something broke'
        );

    check = (id, culture) => this.request('GET', `Check/${id}/${culture} `);

    checkDirty = data => this.request('POST', `CheckDirty`, data);

    getSettings = () => this.request('GET', `GetSettings`);

    saveSettings = (settings, tabs) => this.request('POST', `SaveSettings`, {
        settings,
        tabs
    });
}