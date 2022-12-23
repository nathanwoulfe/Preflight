import { IQService, IRootScopeService } from 'angular';

export class PreflightHub {

  public static serviceName = 'preflightHub';

  scripts: Array<string> = [];
  callbacks: Array<any> = [];
  starting: boolean = false

  constructor(private $rootScope: IRootScopeService, private $q: IQService, private assetsService: IAssetsService) {
    this.scripts = [Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/lib/signalr/signalr.min.js'];
  }

  setupHub = callback => {
    let hub = {};

    $.connection = new signalR.HubConnectionBuilder()
      .withUrl(Umbraco.Sys.ServerVariables.Preflight.signalRHub, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    hub = {
      active: true,
      start: async (callback) => {
        try {
          const result = await $.connection.start();
          this.executeCallback(callback, result);
        } catch (e) {
          console.warn('Could not setup signalR connection', e);
        }

      },
      on: (eventName, callback) => {
        $.connection.on(eventName, result => this.executeCallback(callback, result));
      },

      invoke: (methodName, callback) => {
        $.connection.invoke(methodName)
          .done(result => this.executeCallback(callback, result));
      }
    };

    return callback(hub);
  }

  executeCallback = (cb, result) =>
    this.$rootScope.$apply(() => cb ? cb(result) : {});

  processCallbacks() {
    while (this.callbacks.length) {
      const cb = this.callbacks.pop();
      this.setupHub(cb);
    }

    this.starting = false;
  }

  initHub(callback) {
    this.callbacks.push(callback);

    if (!this.starting) {
      if ($.connection === undefined) {
        this.starting = true;

        const promises: Array<Promise<any>> = [];
        this.scripts.forEach(script => promises.push(this.assetsService.loadJs(script)));

        this.$q.all(promises).then(() => this.processCallbacks());
      } else {
        this.processCallbacks();
      }
    }
  }
}
