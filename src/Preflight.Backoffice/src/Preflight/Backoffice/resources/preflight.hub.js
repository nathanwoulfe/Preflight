export class PreflightHub {

    static serviceName = 'preflightHub';

    $rootScope;
    $q;
    assetsService;

    scripts = [];
    callbacks = [];

    starting = false
    platform = Umbraco.Sys.ServerVariables.Preflight.platform;

    constructor($rootScope, $q, assetsService) {
        this.$rootScope = $rootScope;
        this.$q = $q;
        this.assetsService = assetsService;

        const umbracoPath = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath;
        if (this.platform === 'CORE') {
            this.scripts = [umbracoPath + '/lib/signalr/signalr.min.js'];
        } else {
            this.scripts = [
                umbracoPath + '/lib/signalr/jquery.signalr.js',
                umbracoPath + '/backoffice/signalr/hubs'
            ];
        }
    }

    setupHub = callback => {
        
        let proxy;
        let hub = {};

        if (this.platform === 'CORE') {
            $.connection = new signalR.HubConnectionBuilder()
                .withUrl(Umbraco.Sys.ServerVariables.Preflight.signalRHub)
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Warning)
                .build();

            proxy = $.connection;
        }
        else {
            proxy = $.connection.preflightHub;
        }

        if (proxy !== undefined) {
            hub = {
                active: true,
                start: callback => {
                    if (this.platform === 'CORE') {
                        try {
                            proxy.start()
                                .then(() => callback ? callback() : {})
                                .catch(() => console.warn('Failed to start hub'));
                        } catch (e) {
                            console.warn('Could not setup signalR connection', e);
                        }
                    }
                    else {
                        if ($.connection.hub.state !== $.connection.connectionState.disconnected) {
                            $.connection.hub.stop(true, true);
                        }
                        $.connection.hub.start();
                        callback ? callback() : {};
                    }
                },
                on: (eventName, callback) => {
                    proxy.on(eventName, result => {
                        this.$rootScope.$apply(() => {
                            if (callback) {
                                console.log(result);
                                callback(result);
                            }
                        });
                    });
                }
            };
        } else {
            hub = {
                on: () => { },
                start: () => console.warn('No hub to start'),
            };
        }
        
        return callback(hub);
    }


    /**
     * Function is common across 472 and 5.0
     * */
    processCallbacks() {
        while (this.callbacks.length) {
            const cb = this.callbacks.pop();
            this.setupHub(cb);
        }

        this.starting = false;
    }

    /**
     * Function is common across 472 and 5.0 
     * @param callback
     */
    initHub(callback) {
        this.callbacks.push(callback);

        if (!this.starting) {
            if ($.connection === undefined) {
                this.starting = true;

                const promises = [];
                this.scripts.forEach(script => promises.push(this.assetsService.loadJs(script)));

                this.$q.all(promises).then(() => this.processCallbacks());
            } else {
                this.processCallbacks();
            }
        }
    }
}