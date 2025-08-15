const SESSIONKEY_SANDBOX_HINT = 'mbs-sbhint';
const HINT_TEXT = "This is a test installation of MarBas limited to 300 MB of storage, all data is deleted every 12 hours.";
const HINT_BTNLABEL = 'Close';

export const AppConfig = {
    requires: ['StorageUtils'],
    install: function install(ctx) {
		EnvConfig.apiBaseUrl = `/databroker/api/marbas`;
		let elm = document.getElementById('silo-auth-txt-url');
		if (elm) {
			elm.value = EnvConfig.apiBaseUrl;
		}
        this._showHint(ctx);
    },
    _showHint: function _showHint(ctx) {
        if (ctx.StorageUtils.read(SESSIONKEY_SANDBOX_HINT, false, false)) {
            return;
        }
        const cnt = document.createElement('div');
        cnt.className = 'alert alert-warning alert-dismissible fade show';
        cnt.setAttribute('role', 'alert');
        cnt.textContent = HINT_TEXT;
        
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn-close';
        btn.title = HINT_BTNLABEL;
        btn.setAttribute('data-bs-dismiss', 'alert');
        btn.setAttribute('aria-label', HINT_BTNLABEL);
        cnt.appendChild(btn);
        
        document.getElementById('main').insertBefore(cnt, document.getElementById('top'));
        ctx.StorageUtils.write(SESSIONKEY_SANDBOX_HINT, true);
    }
};