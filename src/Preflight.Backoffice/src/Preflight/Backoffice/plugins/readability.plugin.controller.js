export class ReadabilityPluginController {

    static controllerName = 'readabilityPluginController';

    $scope;
    editorService;

    constructor($scope, editorService) {
        this.$scope = $scope;
        this.editorService = editorService;
    }

    help = e => {
        e.preventDefault();
        const helpOverlay = {
            view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/plugins/readability.overlay.html`,
            title: 'Readability',
            description: 'Why should I care?',
            size: 'small',
            text: this.$scope.model.description,
            close: () => this.editorService.close()                
        };

        this.editorService.open(helpOverlay);
    };
}
