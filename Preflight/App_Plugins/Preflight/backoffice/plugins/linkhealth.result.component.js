(() => {

    const component = {
        transclude: true,
        bindings: {
            results: '<'
        },
        template: `
            <table class="linkhealth-result-table">
                <thead>
                    <tr><th>Link text</th> <th>Link target</th> <th>Link status</th></tr>
                </thead>
                <tr ng-repeat="link in $ctrl.results"><td ng-bind="link.text"></td><td ng-bind="link.href"></td><td ng-bind="link.status"></td></tr>
            </table>`
    };


    angular.module('preflight.components').component('linkhealthResult', component);

})();