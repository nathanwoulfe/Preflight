export class PreflightService implements IPreflightService {

  static serviceName = 'preflightService';

  urlBase: string = Umbraco.Sys.ServerVariables.Preflight.ApiPath;

  constructor(private $http, private umbRequestHelper) {
  }

  request = (method: string, url: string, data?: any) =>
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
