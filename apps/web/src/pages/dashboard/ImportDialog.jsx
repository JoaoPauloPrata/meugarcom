import { useRef, useState } from 'react'
import { api } from '../../lib/api'
import { parsePrice, priceInput } from '../../lib/format'
import Icon from '../../components/Icon'
import { Alert, Dialog, Spinner } from '../../components/ui'

const ACCEPT = '.pdf,.jpg,.jpeg,.png,.webp,.xlsx,.csv'
const MAX_BYTES = 10 * 1024 * 1024
const STEPS = ['Enviar arquivo', 'Revisar', 'Publicar']

/** Menu import: upload → AI extraction (draft) → manual review → publish. */
export default function ImportDialog({ base, onClose, onPublished }) {
  const inputRef = useRef(null)
  const [stage, setStage] = useState('upload') // upload | reading | review
  const [dragging, setDragging] = useState(false)
  const [fileName, setFileName] = useState('')
  const [importId, setImportId] = useState(null)
  const [rows, setRows] = useState([])
  const [error, setError] = useState('')
  const [publishing, setPublishing] = useState(false)

  async function upload(file) {
    setError('')
    if (!file) return
    const extension = file.name.split('.').pop()?.toLowerCase()
    if (!ACCEPT.split(',').includes(`.${extension}`) || file.size > MAX_BYTES) {
      setError('Envie um PDF, uma foto (JPG, PNG, WEBP) ou uma planilha (XLSX, CSV) de até 10 MB.')
      return
    }
    setFileName(file.name)
    setStage('reading')
    const form = new FormData()
    form.append('file', file)
    try {
      const result = await api.upload(`${base}/imports`, form)
      setImportId(result.id)
      setRows(
        result.items.map((item, index) => ({
          key: index,
          categoryName: item.categoryName,
          name: item.name,
          description: item.description ?? '',
          ingredients: item.ingredients ?? '',
          price: item.price > 0 ? priceInput(item.price) : '',
          servesPeople: item.servesPeople,
          portion: item.portion,
          isSpicy: item.isSpicy,
          needsReview: item.needsReview,
        })),
      )
      setStage('review')
    } catch (e) {
      setError(e.message)
      setStage('upload')
    }
  }

  const update = (key, field, value) => setRows((list) => list.map((r) => (r.key === key ? { ...r, [field]: value } : r)))
  const removeRow = (key) => setRows((list) => list.filter((r) => r.key !== key))

  async function publish() {
    setError('')
    const invalid = rows.find((r) => !r.name.trim() || !r.categoryName.trim() || parsePrice(r.price) === null)
    if (invalid) {
      setError(`Revise "${invalid.name || 'item sem nome'}": nome, categoria e preço são obrigatórios.`)
      return
    }
    setPublishing(true)
    try {
      const count = await api.post(`${base}/imports/${importId}/publish`, {
        items: rows.map((r) => ({
          categoryName: r.categoryName.trim(),
          name: r.name.trim(),
          description: r.description.trim() || null,
          ingredients: r.ingredients.trim() || null,
          price: parsePrice(r.price),
          servesPeople: r.servesPeople ?? null,
          portion: r.portion ?? null,
          isSpicy: r.isSpicy,
        })),
      })
      onPublished(count)
    } catch (e) {
      setError(e.message)
      setPublishing(false)
    }
  }

  const stepIndex = stage === 'review' ? 1 : 0
  const flagged = rows.filter((r) => r.needsReview).length

  return (
    <Dialog
      title="Importar cardápio"
      onClose={onClose}
      footer={
        stage === 'review' ? (
          <>
            <button type="button" className="btn btn--ghost" onClick={() => setStage('upload')} disabled={publishing}>
              Enviar outro arquivo
            </button>
            <button type="button" className="btn btn--primary" onClick={publish} disabled={publishing || rows.length === 0}>
              {publishing ? <Spinner label="Publicando" /> : `Publicar ${rows.length} itens`}
            </button>
          </>
        ) : null
      }
    >
      <div className="import-steps" aria-label="Etapas da importação">
        {STEPS.map((s, i) => (
          <span key={s} className={i === stepIndex ? 'is-current' : ''} aria-current={i === stepIndex ? 'step' : undefined}>
            {i > 0 && '· '}
            {s}
          </span>
        ))}
      </div>
      <Alert>{error}</Alert>

      {stage === 'upload' && (
        <label
          className={`dropzone ${dragging ? 'is-dragging' : ''}`}
          onDragOver={(e) => {
            e.preventDefault()
            setDragging(true)
          }}
          onDragLeave={() => setDragging(false)}
          onDrop={(e) => {
            e.preventDefault()
            setDragging(false)
            upload(e.dataTransfer.files?.[0])
          }}
        >
          <Icon name="upload" size={32} />
          <strong>Arraste o PDF, foto ou planilha do seu cardápio</strong>
          <span>ou clique para escolher · PDF, JPG, PNG, WEBP, XLSX, CSV · até 10 MB</span>
          <input
            ref={inputRef}
            type="file"
            accept={ACCEPT}
            className="sr-only"
            onChange={(e) => upload(e.target.files?.[0])}
            data-autofocus
          />
        </label>
      )}

      {stage === 'reading' && (
        <div className="card" role="status" aria-live="polite">
          <strong>Lendo seu cardápio com IA…</strong>
          <div className="progress" aria-hidden="true">
            <div className="progress-bar" />
          </div>
          <span className="hint">{fileName} · pode levar até um minuto em cardápios grandes.</span>
        </div>
      )}

      {stage === 'review' && (
        <>
          <Alert kind="info">
            Encontramos <strong>{rows.length} itens</strong>. Confira nomes, categorias e preços antes de publicar.
            {flagged > 0 && ` ${flagged} itens destacados precisam de atenção (preço ausente ou texto suspeito).`} Vegano, sem lactose e sem
            glúten nunca são marcados na importação: revise na edição de cada prato.
          </Alert>
          <div className="review-table-wrap">
            <table className="review-table">
              <thead>
                <tr>
                  <th scope="col" className="col-cat">Categoria</th>
                  <th scope="col">Nome</th>
                  <th scope="col" className="col-price">Preço (R$)</th>
                  <th scope="col">Descrição / ingredientes</th>
                  <th scope="col">
                    <span className="sr-only">Remover</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r) => (
                  <tr key={r.key} className={r.needsReview ? 'needs-review' : ''}>
                    <td>
                      <input
                        className="input"
                        aria-label={`Categoria de ${r.name}`}
                        value={r.categoryName}
                        onChange={(e) => update(r.key, 'categoryName', e.target.value)}
                      />
                    </td>
                    <td>
                      <input className="input" aria-label="Nome" value={r.name} onChange={(e) => update(r.key, 'name', e.target.value)} />
                      {r.needsReview && <span className="diet-tag diet-tag--warn" style={{ marginTop: 6 }}>Revisar</span>}
                    </td>
                    <td>
                      <input
                        className="input"
                        inputMode="decimal"
                        aria-label={`Preço de ${r.name}`}
                        value={r.price}
                        aria-invalid={parsePrice(r.price) === null ? 'true' : undefined}
                        onChange={(e) => update(r.key, 'price', e.target.value)}
                      />
                    </td>
                    <td>
                      <input
                        className="input"
                        aria-label={`Descrição de ${r.name}`}
                        value={r.description}
                        placeholder="Descrição"
                        onChange={(e) => update(r.key, 'description', e.target.value)}
                      />
                      <input
                        className="input"
                        style={{ marginTop: 6 }}
                        aria-label={`Ingredientes de ${r.name}`}
                        value={r.ingredients}
                        placeholder="Ingredientes"
                        onChange={(e) => update(r.key, 'ingredients', e.target.value)}
                      />
                    </td>
                    <td>
                      <button type="button" className="icon-btn icon-btn--plain" onClick={() => removeRow(r.key)} aria-label={`Remover ${r.name}`}>
                        <Icon name="trash" size={18} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </Dialog>
  )
}
