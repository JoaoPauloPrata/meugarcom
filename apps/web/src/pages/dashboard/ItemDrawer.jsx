import { useEffect, useRef, useState } from 'react'
import { api } from '../../lib/api'
import { parsePrice, priceInput } from '../../lib/format'
import { ACCEPTED_IMAGE_TYPES, MAX_IMAGE_BYTES, prepareMenuImage } from '../../lib/image'
import Icon from '../../components/Icon'
import { Alert, Dialog, Field, Spinner } from '../../components/ui'

const EMPTY = {
  name: '',
  description: '',
  ingredients: '',
  price: '',
  servesPeople: '',
  portion: '',
  isSpicy: false,
  isVegan: false,
  isLactoseFree: false,
  isGlutenFree: false,
}

/** Create / edit a dish. The dietary checkboxes are the manual review required by the guardrails. */
export default function ItemDrawer({ base, item, defaultCategoryId, categories, onClose, onSaved }) {
  const [form, setForm] = useState(() =>
    item
      ? {
          ...EMPTY,
          ...item,
          description: item.description ?? '',
          ingredients: item.ingredients ?? '',
          portion: item.portion ?? '',
          price: priceInput(item.price),
          servesPeople: item.servesPeople ?? '',
        }
      : { ...EMPTY },
  )
  const [categoryId, setCategoryId] = useState(item?.categoryId ?? defaultCategoryId ?? categories[0]?.id)
  const [errors, setErrors] = useState({})
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [confirmDelete, setConfirmDelete] = useState(false)
  // keep: unchanged | new: picked locally, uploaded on save | remove: cleared on save
  const [photo, setPhoto] = useState({ kind: 'keep' })
  const [photoError, setPhotoError] = useState('')
  const [preparingPhoto, setPreparingPhoto] = useState(false)
  const fileRef = useRef(null)

  useEffect(() => () => photo.previewUrl && URL.revokeObjectURL(photo.previewUrl), [photo])

  const photoUrl = photo.kind === 'new' ? photo.previewUrl : photo.kind === 'remove' ? null : (item?.imageUrl ?? null)

  async function pickPhoto(e) {
    const file = e.target.files?.[0]
    e.target.value = '' // lets the same file be picked again after removing it
    if (!file) return
    setPhotoError('')
    if (!ACCEPTED_IMAGE_TYPES.includes(file.type) || file.size > 20 * 1024 * 1024) {
      setPhotoError('Use uma foto JPG, PNG ou WEBP.')
      return
    }
    setPreparingPhoto(true)
    try {
      const blob = await prepareMenuImage(file)
      if (blob.size > MAX_IMAGE_BYTES) {
        setPhotoError('A foto ficou maior que 5 MB. Tente outra imagem.')
        return
      }
      setPhoto({ kind: 'new', blob, previewUrl: URL.createObjectURL(blob) })
    } catch {
      setPhotoError('Não consegui ler essa foto. Tente outra imagem.')
    } finally {
      setPreparingPhoto(false)
    }
  }

  async function savePhoto(itemId) {
    if (photo.kind === 'new') {
      const form = new FormData()
      const extension = photo.blob.type === 'image/webp' ? 'webp' : 'jpg'
      form.append('file', photo.blob, `photo.${extension}`)
      await api.upload(`${base}/items/${itemId}/image`, form, 'PUT')
    } else if (photo.kind === 'remove' && item?.imageUrl) {
      await api.del(`${base}/items/${itemId}/image`)
    }
  }

  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.type === 'checkbox' ? e.target.checked : e.target.value }))

  async function save() {
    setError('')
    const price = parsePrice(form.price)
    const serves = form.servesPeople === '' ? null : parseInt(form.servesPeople, 10)
    const next = {}
    if (!form.name.trim()) next.name = 'Informe o nome do prato.'
    if (price === null) next.price = 'Informe um preço válido, por exemplo 38,90.'
    if (serves !== null && (!Number.isFinite(serves) || serves < 1 || serves > 50)) next.servesPeople = 'Use um número de 1 a 50.'
    setErrors(next)
    if (Object.keys(next).length) return

    const body = {
      categoryId,
      name: form.name.trim(),
      description: form.description.trim() || null,
      ingredients: form.ingredients.trim() || null,
      price,
      servesPeople: serves,
      portion: form.portion.trim() || null,
      isSpicy: form.isSpicy,
      isVegan: form.isVegan,
      isLactoseFree: form.isLactoseFree,
      isGlutenFree: form.isGlutenFree,
    }

    setBusy(true)
    let itemId = item?.id
    try {
      if (item) await api.put(`${base}/items/${item.id}`, body)
      else itemId = await api.post(`${base}/items`, body)
    } catch (e) {
      setError(e.message)
      setBusy(false)
      return
    }

    try {
      await savePhoto(itemId)
      onSaved(item ? 'Prato atualizado.' : 'Prato criado.')
    } catch (e) {
      // The dish itself is saved; only the photo failed, so say that instead of losing the edit.
      onSaved(`${item ? 'Prato atualizado' : 'Prato criado'}, mas a foto não foi salva: ${e.message}`)
    }
  }

  async function remove() {
    setBusy(true)
    try {
      await api.del(`${base}/items/${item.id}`)
      onSaved('Prato excluído.')
    } catch (e) {
      setError(e.message)
      setBusy(false)
    }
  }

  return (
    <Dialog
      title={item ? 'Editar prato' : 'Novo prato'}
      variant="drawer"
      onClose={onClose}
      footer={
        confirmDelete ? (
          <>
            <span style={{ alignSelf: 'center' }}>Excluir este prato?</span>
            <span className="spacer" />
            <button type="button" className="btn btn--ghost" onClick={() => setConfirmDelete(false)}>
              Cancelar
            </button>
            <button type="button" className="btn btn--danger" onClick={remove} disabled={busy}>
              Excluir
            </button>
          </>
        ) : (
          <>
            {item && (
              <button type="button" className="btn btn--danger" onClick={() => setConfirmDelete(true)}>
                <Icon name="trash" size={16} />
                Excluir
              </button>
            )}
            <span className="spacer" />
            <button type="button" className="btn btn--ghost" onClick={onClose}>
              Cancelar
            </button>
            <button type="button" className="btn btn--primary" onClick={save} disabled={busy || preparingPhoto}>
              {busy ? <Spinner label="Salvando" /> : 'Salvar'}
            </button>
          </>
        )
      }
    >
      <Alert>{error}</Alert>
      <form
        onSubmit={(e) => {
          e.preventDefault()
          save()
        }}
        noValidate
      >
        <div className="photo-field">
          <span className="photo-field-label" id="item-photo-label">
            Foto <span className="optional">(opcional)</span>
          </span>
          <div className="photo-field-body">
            {photoUrl ? (
              <img className="photo-preview" src={photoUrl} alt={`Foto de ${form.name || 'prato'}`} width="96" height="96" />
            ) : (
              <span className="photo-preview photo-preview--empty" aria-hidden="true">
                {preparingPhoto ? <Spinner label="Preparando foto" /> : <Icon name="image" size={28} />}
              </span>
            )}
            <div className="photo-field-actions">
              <input
                ref={fileRef}
                type="file"
                accept={ACCEPTED_IMAGE_TYPES.join(',')}
                className="sr-only"
                tabIndex={-1}
                aria-labelledby="item-photo-label"
                onChange={pickPhoto}
              />
              <button type="button" className="btn btn--ghost btn--sm" onClick={() => fileRef.current?.click()} disabled={preparingPhoto}>
                <Icon name="upload" size={16} />
                {photoUrl ? 'Trocar foto' : 'Escolher foto'}
              </button>
              {photoUrl && (
                <button
                  type="button"
                  className="btn btn--ghost btn--sm"
                  onClick={() => {
                    setPhoto({ kind: 'remove' })
                    setPhotoError('')
                  }}
                >
                  <Icon name="trash" size={16} />
                  Remover
                </button>
              )}
              <p className={photoError ? 'field-error' : 'hint'} role={photoError ? 'alert' : undefined}>
                {photoError || 'JPG, PNG ou WEBP. A foto aparece para o cliente no chat.'}
              </p>
            </div>
          </div>
        </div>
        <Field label="Nome" value={form.name} onChange={set('name')} error={errors.name} maxLength={120} data-autofocus />
        <Field as="select" label="Categoria" value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </Field>
        <div className="field-row" style={{ marginTop: 16 }}>
          <Field label="Preço (R$)" inputMode="decimal" value={form.price} onChange={set('price')} error={errors.price} placeholder="38,90" />
          <Field label="Porção" value={form.portion} onChange={set('portion')} maxLength={60} placeholder="Ex.: 400 g" />
        </div>
        <Field
          label="Serve quantas pessoas"
          type="number"
          inputMode="numeric"
          min={1}
          max={50}
          value={form.servesPeople}
          onChange={set('servesPeople')}
          error={errors.servesPeople}
          hint="Deixe em branco se o cardápio não informa."
        />
        <Field
          as="textarea"
          label="Descrição"
          value={form.description}
          onChange={set('description')}
          maxLength={1000}
        />
        <Field
          as="textarea"
          label="Ingredientes"
          value={form.ingredients}
          onChange={set('ingredients')}
          maxLength={1000}
          hint="O garçom de IA só cita ingredientes escritos aqui ou na descrição."
        />
        <label className="checkbox">
          <input type="checkbox" checked={form.isSpicy} onChange={set('isSpicy')} />
          <span>Apimentado</span>
        </label>

        <fieldset className="fieldset">
          <legend>Restrições alimentares (revisão manual)</legend>
          <p className="hint">
            Marque só o que você confirmou. Com a caixa marcada, o garçom de IA afirma; sem ela, ele não afirma nada e pede para o
            cliente chamar o garçom.
          </p>
          <label className="checkbox">
            <input type="checkbox" checked={form.isVegan} onChange={set('isVegan')} />
            <span>Vegano</span>
          </label>
          <label className="checkbox">
            <input type="checkbox" checked={form.isLactoseFree} onChange={set('isLactoseFree')} />
            <span>Sem lactose</span>
          </label>
          <label className="checkbox">
            <input type="checkbox" checked={form.isGlutenFree} onChange={set('isGlutenFree')} />
            <span>Sem glúten</span>
          </label>
        </fieldset>
        <button type="submit" hidden />
      </form>
    </Dialog>
  )
}
