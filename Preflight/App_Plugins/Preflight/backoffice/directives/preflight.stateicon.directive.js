(() => {

    const template = `
        <div class="state-icon {{ className }}">
            <i class="icon icon-{{ icon }}"></i>
        </div>`;

    function preflightStateIcon() {
        const dir = {
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

                scope.$watch('failed',
                    () => {
                        if (!scope.disabled) {
                            scope.icon = scope.failed ? 'delete' : 'check';
                            scope.className = scope.failed ? 'fail' : 'pass';
                        }
                    });
            }
        };

        return dir;
    }

    angular.module('preflight.directives').directive('preflightStateIcon', preflightStateIcon);

})();