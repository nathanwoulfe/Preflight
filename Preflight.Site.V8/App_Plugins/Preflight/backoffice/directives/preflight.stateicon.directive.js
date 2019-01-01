(() => {

    const template = `
        <div class="state-icon {{ className }}">
            <i class="icon icon-{{ icon }}"></i>
        </div>`;

    function preflightStateIcon() {
        const card = {
            restrict: 'E',
            scope: {
                failed: '=',
                disabled: '='
            },
            replace: true,
            template: template,
            link: scope => {

                scope.icon = 'power';
                scope.className = 'disabled';

                if (!scope.disabled) {
                    scope.icon = scope.failed ? 'delete' : 'check';
                    scope.className = scope.failed ? 'fail' : 'pass';
                } 
            }
        };

        return card;
    }

    angular.module('umbraco').directive('preflightStateIcon', preflightStateIcon);

})();