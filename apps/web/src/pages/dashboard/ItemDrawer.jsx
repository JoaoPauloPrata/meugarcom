import { useState } from 'react'
import { api } from '../../lib/api'
import { parsePrice, priceInput } from '../../lib/format'
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
    try {
      if (item) await api.put(`${base}/items/${item.id}`, body)
      else await api.post(`${base}/items`, body)
      onSaved(item ? 'Prato atualizado.' : 'Prato criado.')
    } catch (e) {
      setError(e.message)
      setBusy(false)
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
            <button type="button" className="btn btn--primary" onClick={save} disabled={busy}>
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
