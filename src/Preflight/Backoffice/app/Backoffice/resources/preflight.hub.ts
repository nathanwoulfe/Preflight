import { IRootScopeService } from 'angular';

export class PreflightHub {

  public static serviceName = 'preflightHub';

  callbacks: Array<any> = [];
  starting: boolean = false;
  hubStarted: boolean = false;
  hub = {};

  constructor(private $rootScope: IRootScopeService, private assetsService: IAssetsService) {
  }

  setupHub = callback => {

    // process callbacks every time the hub is re-initialized
    // but only create the connection the first time.
    if ($.preflightHubConnection !== undefined || this.hubStarted) {
      return callback(this.hub);
    }

    $.preflightHubConnection = new signalR.HubConnectionBuilder()
      .withUrl(Umbraco.Sys.ServerVariables.Preflight.signalRHub)
      .configureLogging(Umbraco.Sys.ServerVariables.isDebuggingEnabled ? signalR.LogLevel.Debug : signalR.LogLevel.None)
      .withAutomaticReconnect()
      .build();

    this.hub = {
      start: async (callback) => {
        if (this.hubStarted) {
          return callback(this.hub);
        }

        try {
          const result = await $.preflightHubConnection.start();
          this.executeCallback(callback, result);
          this.hubStarted = true;
        } catch (e) {
          console.warn('Could not setup signalR connection', e);
        }

      },
      on: (eventName, callback) => {
        $.preflightHubConnection.on(eventName, result => this.executeCallback(callback, result));
      },
    };

    return callback(this.hub);
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
      if ($.workflowHubConnection === undefined) {
        this.starting = true;

        this.assetsService.loadJs(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/lib/signalr/signalr.min.js')
          .then(() => this.processCallbacks());
      }
      else {
        this.processCallbacks();
      }
    }
  }
}
