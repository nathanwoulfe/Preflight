class StateIcon {

    static template = `
        <div class="state-icon {{ ::$ctrl.className }}">
            <i class="icon icon-{{ ::$ctrl.icon }}"></i>
        </div>`;

    icon = 'power';
    className = 'disabled';

    $onInit() {
        if (!this.disabled) {
            this.icon = this.failed ? 'delete' : 'check';
            this.className = this.failed ? 'fail' : 'pass';
        }
    };
}

export const StateIconComponent = {
    transclude: true,
    name: 'stateIcon',
    bindings: {
        failed: '<',
        disabled: '<'
    },
    template: StateIcon.template,
    controller: StateIcon,
};
