const Request = window.Request
const Headers = window.Headers
const fetch = window.fetch

class Api {
    async request(method, url, body) {
        if (body) {
            body = JSON.stringify(body)
        }

        const request = new Request('/api/' + url, {
            method: method,
            body: body,
            credentials: 'same-origin',
            headers: new Headers({
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            })
        })

        const resp = await fetch(request)
        if (!resp.ok && resp.status !== 400) {
            throw Error(resp.statusText)
        }

        const jsonResult = await resp.json()

        if (resp.status === 400) {
            jsonResult.requestStatus = 400
        }

        return jsonResult
    }

    async getDomain(domainOrIp) {
        return this.request('GET', `domain/${domainOrIp}`)
    }
}

const api = new Api()

var callback = () => {
    const btn = document.getElementById('btn-search')
    const txt = document.getElementById('txt-search')
    const result = document.getElementById('whois-results')

    if (btn) {
        btn.onclick = async () => {
            const originalBtnText = btn.innerHTML;
            btn.innerHTML = '<i class="icon icon-search icon-white mr-1"></i><span>Processando...</span>';
            btn.disabled = true;
            result.innerHTML = '';
            result.classList.remove('hide');

            try {
                const response = await api.getDomain(txt.value)

                if (response && response.requestStatus === 400) {
                    result.innerHTML = `
                                        <div class="col-12 mt-4">
                                            <div class="alert alert-warning text-center">
                                                <strong>Atenção!</strong> O domínio informado é inválido. Tente novamente (ex: umbler.com).
                                            </div>
                                        </div>
                                       `;
                    return;
                }

                if (response) {
                    const formattedWhois = response.whoIs ? response.whoIs.replace(/\n/g, '<br>') : 'Nenhum dado de Whois retornado.';

                    const cardHtml = `
                                       <div class="col-12 mt-4">
                                           <div class="card shadow border-success" style="border-radius: 8px; overflow: hidden;">
                                               <div class="card-header bg-success text-white">
                                                   <h4 class="mb-0" style="color: white; font-weight: bold;">
                                                      Resultados para: <u>${response.name}</u>
                                                   </h4>
                                               </div>
                                               <div class="card-body">
                                                   <div class="row mb-4 text-center">
                                                       <div class="col-md-6">
                                                           <p class="mb-1 text-muted" style="text-transform: uppercase; font-size: 0.8em; font-weight: bold;">Endereco IP</p>
                                                           <h3><span class="badge badge-info" style="background-color: #17a2b8;">${response.ip || 'Não resolvido'}</span></h3>
                                                       </div>
                                                       <div class="col-md-6">
                                                           <p class="mb-1 text-muted" style="text-transform: uppercase; font-size: 0.8em; font-weight: bold;">Empresa de Hospedagem</p>
                                                           <h3><span class="badge badge-warning" style="background-color: #ffc107; color: #333;">${response.hostedAt || 'Desconhecido'}</span></h3>
                                                       </div>
                                                   </div>
                                                   <hr>
                                                   <h5 class="text-muted mb-3"><i class="icon icon-list"></i> Dados Brutos do Whois</h5>
                                                   <div class="bg-light p-3 rounded" style="max-height: 400px; overflow-y: auto; font-family: monospace; font-size: 0.85em; border: 1px solid #dee2e6;">
                                                       ${formattedWhois}
                                                   </div>
                                               </div>
                                           </div>
                                       </div>
                                    `;
                    result.innerHTML = cardHtml;
                }
            } catch (error) {
                result.innerHTML = `
                                    <div class="col-12 mt-4">
                                        <div class="alert alert-danger text-center">
                                            Ocorreu um erro ao comunicar com a API. Verifique sua conexão.
                                        </div>
                                    </div>
                                   `;
            } finally {
                btn.innerHTML = originalBtnText;
                btn.disabled = false;
            }
        }
    }
}

if (document.readyState === 'complete' || (document.readyState !== 'loading' && !document.documentElement.doScroll)) {
    callback()
} else {
    document.addEventListener('DOMContentLoaded', callback)
}