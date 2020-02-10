(() => {
    function ProgressCircleDirective() {

        function link(scope, element) {
            function onInit() {

                // making sure we get the right numbers
                let percent = Math.round(scope.percentage);
                percent = percent > 100 ? 100 : percent || 0;

                // calculating the circle's highlight
                const r = element.find('.umb-progress-circle__highlight').attr('r');
                const pathLength = r * Math.PI * 2 * percent / 100;

                // Full circle length
                scope.dashArray = `${pathLength},100`;

                // set font size
                scope.percentageSize = scope.size * 0.3 + 'px';

                // use rounded percentage
                scope.label = `${percent}%`;
            }

            scope.$watch('percentage', onInit);
        }

        let  directive = {
            restrict: 'E',
            replace: true,
            template: `
                <div class="umb-progress-circle preflight-progress" ng-style="{'width': size, 'height': size }"> {{ percent }}
                    <svg class="umb-progress-circle__view-box" viewBox="0 0 33.83098862 33.83098862"> 
                        <circle class="umb-progress-circle__highlight--{{ background }}" cx="16.91549431" cy="16.91549431" r="15.91549431" fill="none" stroke-width=".5"></circle>
                        <circle class="umb-progress-circle__highlight umb-progress-circle__highlight--{{ foreground }}" 
                            cx="16.91549431" cy="16.91549431" r="15.91549431" stroke-linecap="round" fill="none" stroke-width="2" stroke-dasharray="{{ dashArray }}"></circle> 
                    </svg> 
                    <div ng-style="{'font-size': percentageSize}" class="umb-progress-circle__percentage">
                        {{ label }}
                        <small>pass rate</small>                
                    </div>
                </div>`,
            scope: {
                size: '@?',
                percentage: '=',
                done: '@',
                foreground: '@',
                background: '@'
            },
            link: link
        };

        return directive;
    }

    angular.module('preflight.components').directive('progressCircle', ProgressCircleDirective);
})();