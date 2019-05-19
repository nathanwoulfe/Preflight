(() => {

    const template = `
        <div class="state-icon {{ ::$ctrl.className }}">
            <i class="icon icon-{{ ::$ctrl.icon }}"></i>
        </div>`;

    function controller() {
        this.icon = 'power';
        this.className = 'disabled';

        this.$onInit = () => {
            if (!this.disabled) {
                this.icon = this.failed ? 'delete' : 'check';
                this.className = this.failed ? 'fail' : 'pass';
            }
        };
    }

    const component = {
        transclude: true,
        bindings: {
            failed: '<',
            disabled: '<'
        },
        template: template,
        controller: controller
    };

    angular.module('preflight.components').component('preflightStateIcon', component);

})();