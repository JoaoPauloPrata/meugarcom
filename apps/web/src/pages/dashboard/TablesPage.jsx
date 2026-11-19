import { useCallback, useEffect, useState } from 'react'
import { useOutletContext } from 'react-router-dom'
import QRCode from 'qrcode'
import { api } from '../../lib/api'
import { chatUrl } from '../../lib/format'
import Icon from '../../components/Icon'
import { Alert, Dialog, NumberStepper, Spinner, useToast } from '../../components/ui'

const QR_OPTIONS = { margin: 1, width: 480, errorCorrectionLevel: 'M', color: { dark: '#2a2d31', light: '#ffffff' } }

function useQr(url) {
  const [src, setSrc] = useState('')
  useEffect(() => {
    let active = true
    QRCode.toDataURL(url, QR_OPTIONS).then((data) => active && setSrc(data))
    return () => {
      active = false
    }
  }, [url])
  return src
}

export default function TablesPage() {
  const { unit, workspace } = useOutletContext()
  const [data, setData] = useState(null)
  const [error, setError] = useState('')
  const [adding, setAdding] = useState(false)
  const [removing, setRemoving] = useState(null)
  const [toast, showToast] = useToast()

  const load = useCallback(async () => {
    try {
      setData(await api.get(`/api/units/${unit.id}/tables`))
      setError('')
    } catch (e) {
      setError(e.message)
    }
  }, [unit.id])

  useEffect(() => {
    load()
  }, [load])

  async function copy(url) {
    try {
      await navigator.clipboard.writeText(url)
      showToast('Link copiado.')
    } catch {
      showToast('Não foi possível copiar. Selecione o link e copie manualmente.')
    }
  }

  async function removeTable(table) {
    try {
      await api.del(`/api/units/${unit.id}/tables/${table.id}`)
      setRemoving(null)
      showToast(`Mesa ${table.number} removida.`)
      load()
    } catch (e) {
      setRemoving(null)
      showToast(e.message)
    }
  }

  return (
    <>
      <div className="page-head no-print">
        <div>
          <h1>Mesas e QR</h1>
          <p>Cada QR code abre uma conversa nova com o garçom de IA, já identificando a mesa.</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn btn--ghost" onClick={() => window.print()} disabled={!data?.tables.length}>
            <Icon name="printer" size={18} />
            Imprimir QR codes
          </button>
          <button type="button" className="btn btn--primary" onClick={() => setAdding(true)}>
            <Icon name="plus" size={18} />
            Adicionar mesas
          </button>
        </div>
      </div>
      <Alert>{error}</Alert>

      {!data ? (
        !error && <Spinner label="Carregando mesas" />
      ) : (
        <>
          <UnitLinkCard code={data.unitPublicCode} onCopy={copy} />
          {data.tables.length === 0 ? (
            <div className="card empty">
              <h2>Nenhuma mesa ainda</h2>
              <p>Adicione as mesas para gerar um QR code para cada uma.</p>
              <button type="button" className="btn btn--primary" onClick={() => setAdding(true)}>
                Adicionar mesas
              </button>
            </div>
          ) : (
            <div className="print-area">
              <p className="print-only" style={{ fontFamily: 'var(--serif)', fontSize: 24, marginBottom: 16 }}>
                {workspace.restaurantName}
              </p>
              <div className="qr-grid">
                {data.tables.map((table) => (
                  <TableCard key={table.id} table={table} onCopy={copy} onRemove={() => setRemoving(table)} />
                ))}
              </div>
            </div>
          )}
        </>
      )}

      {adding && (
        <AddTablesDialog
          unitId={unit.id}
          onClose={() => setAdding(false)}
          onDone={(count) => {
            setAdding(false)
            showToast(`${count} ${count === 1 ? 'mesa adicionada' : 'mesas adicionadas'}.`)
            load()
          }}
        />
      )}

      {removing && (
        <Dialog
          title="Remover mesa"
          variant="modal-sm"
          onClose={() => setRemoving(null)}
          footer={
            <>
              <button type="button" className="btn btn--ghost" onClick={() => setRemoving(null)}>
                Cancelar
              </button>
              <button type="button" className="btn btn--danger" onClick={() => removeTable(removing)}>
                Remover
              </button>
            </>
          }
        >
          <p>
            Remover a <strong>Mesa {removing.number}</strong>? O QR code impresso dela para de funcionar.
          </p>
        </Dialog>
      )}
      {toast}
    </>
  )
}

function UnitLinkCard({ code, onCopy }) {
  const url = chatUrl(code)
  const qr = useQr(url)
  return (
    <section className="card link-card no-print" aria-labelledby="unit-link-title">
      {qr ? <img src={qr} alt="QR code do link geral do restaurante" /> : <span className="spinner" />}
      <div>
        <h2 id="unit-link-title">Link geral do cardápio</h2>
        <p>Para balcão, delivery ou redes sociais. Abre o chat sem identificar a mesa.</p>
        <code className="link-url">{url}</code>
        <div className="page-actions">
          <button type="button" className="btn btn--ghost btn--sm" onClick={() => onCopy(url)}>
            <Icon name="copy" size={16} />
            Copiar link
          </button>
          <a className="btn btn--ghost btn--sm" href={url} target="_blank" rel="noreferrer">
            <Icon name="external" size={16} />
            Abrir chat
          </a>
        </div>
      </div>
    </section>
  )
}

function TableCard({ table, onCopy, onRemove }) {
  const url = chatUrl(table.publicCode)
  const qr = useQr(url)
  return (
    <article className="qr-card">
      {qr ? <img src={qr} alt={`QR code da mesa ${table.number}`} /> : <span className="spinner" />}
      <h3>Mesa {table.number}</h3>
      <div className="qr-actions">
        <a className="icon-btn" href={qr} download={`mesa-${table.number}.png`} aria-label={`Baixar QR code da mesa ${table.number}`}>
          <Icon name="download" size={18} />
        </a>
        <button type="button" className="icon-btn" onClick={() => onCopy(url)} aria-label={`Copiar link da mesa ${table.number}`}>
          <Icon name="copy" size={18} />
        </button>
        <a className="icon-btn" href={url} target="_blank" rel="noreferrer" aria-label={`Abrir o chat da mesa ${table.number}`}>
          <Icon name="external" size={18} />
        </a>
        <button type="button" className="icon-btn" onClick={onRemove} aria-label={`Remover mesa ${table.number}`}>
          <Icon name="trash" size={18} />
        </button>
      </div>
    </article>
  )
}

function AddTablesDialog({ unitId, onClose, onDone }) {
  const [count, setCount] = useState(1)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function submit() {
    setBusy(true)
    setError('')
    try {
      const added = await api.post(`/api/units/${unitId}/tables`, { count })
      onDone(added)
    } catch (e) {
      setError(e.message)
      setBusy(false)
    }
  }

  return (
    <Dialog
      title="Adicionar mesas"
      variant="modal-sm"
      onClose={onClose}
      footer={
        <>
          <button type="button" className="btn btn--ghost" onClick={onClose}>
            Cancelar
          </button>
          <button type="button" className="btn btn--primary" onClick={submit} disabled={busy}>
            {busy ? <Spinner label="Adicionando" /> : 'Adicionar'}
          </button>
        </>
      }
    >
      <Alert>{error}</Alert>
      <div className="tables-field">
        <span>Quantas mesas?</span>
        <NumberStepper label="Quantidade de mesas" value={count} min={1} max={300} onChange={setCount} />
      </div>
      <p className="hint" style={{ marginTop: 12, color: 'var(--muted)' }}>
        As novas mesas continuam a numeração das existentes.
      </p>
    </Dialog>
  )
}
