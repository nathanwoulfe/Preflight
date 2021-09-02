export class ReadabilityPluginController {

    static controllerName = 'readability.plugin.controller';

    $scope;
    overlayService;

    constructor($scope, overlayService) {
        this.$scope = $scope;
        this.overlayService = overlayService;
    }

    help = () => {
        const overlay = {
            view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/plugins/readability/readability.overlay.html`,
            title: this.$scope.model.name,
            size: 'medium',
            content: this.$scope.model.description,
            close: () => this.overlayService.close(),                
        };

        this.overlayService.open(overlay);
    };
}
