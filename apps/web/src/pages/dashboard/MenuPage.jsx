import { useCallback, useEffect, useMemo, useState } from 'react'
import { useOutletContext } from 'react-router-dom'
import { api } from '../../lib/api'
import { DIETARY_LABELS, formatBRL, itemTags } from '../../lib/format'
import Icon from '../../components/Icon'
import { Alert, Dialog, Field, Spinner, Switch, useToast } from '../../components/ui'
import ItemDrawer from './ItemDrawer'
import ImportDialog from './ImportDialog'

export default function MenuPage() {
  const { unit } = useOutletContext()
  const base = `/api/units/${unit.id}/menu`
  const [menu, setMenu] = useState(null)
  const [error, setError] = useState('')
  const [activeId, setActiveId] = useState(null)
  const [editing, setEditing] = useState(null) // item | { new: true }
  const [categoryDialog, setCategoryDialog] = useState(null) // { mode: 'create' | 'rename' | 'delete', category? }
  const [importing, setImporting] = useState(false)
  const [toast, showToast] = useToast()

  const load = useCallback(async () => {
    try {
      const data = await api.get(base)
      setMenu(data)
      setError('')
      setActiveId((current) =>
        data.categories.some((c) => c.id === current) ? current : (data.categories[0]?.id ?? null),
      )
    } catch (e) {
      setError(e.message)
    }
  }, [base])

  useEffect(() => {
    load()
  }, [load])

  const active = useMemo(() => menu?.categories.find((c) => c.id === activeId) ?? null, [menu, activeId])
  const totalItems = menu?.categories.reduce((sum, c) => sum + c.items.length, 0) ?? 0

  async function toggleAvailability(item, isAvailable) {
    setMenu((m) => ({
      ...m,
      categories: m.categories.map((c) => ({
        ...c,
        items: c.items.map((i) => (i.id === item.id ? { ...i, isAvailable } : i)),
      })),
    }))
    try {
      await api.patch(`${base}/items/${item.id}/availability`, { isAvailable })
      showToast(isAvailable ? `${item.name} voltou para o chat.` : `${item.name} saiu do chat.`)
    } catch (e) {
      showToast(e.message)
      load()
    }
  }

  if (!menu) {
    return (
      <>
        <div className="page-head">
          <h1>Cardápio</h1>
        </div>
        {error ? <Alert>{error}</Alert> : <Spinner label="Carregando cardápio" />}
      </>
    )
  }

  return (
    <>
      <div className="page-head">
        <div>
          <h1>Cardápio</h1>
          <p>
            {totalItems} {totalItems === 1 ? 'item' : 'itens'} em {menu.categories.length}{' '}
            {menu.categories.length === 1 ? 'categoria' : 'categorias'}
          </p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn btn--ghost" onClick={() => setImporting(true)}>
            <Icon name="upload" size={18} />
            Importar cardápio
          </button>
          <button
            type="button"
            className="btn btn--primary"
            onClick={() => (menu.categories.length ? setEditing({ new: true }) : setCategoryDialog({ mode: 'create' }))}
          >
            <Icon name="plus" size={18} />
            Novo prato
          </button>
        </div>
      </div>
      <Alert>{error}</Alert>

      {menu.categories.length === 0 ? (
        <div className="card empty">
          <h2>Seu cardápio está vazio</h2>
          <p>Importe o cardápio que você já tem (PDF, foto ou planilha) e revise antes de publicar. Ou comece criando uma categoria.</p>
          <div className="page-actions">
            <button type="button" className="btn btn--primary" onClick={() => setImporting(true)}>
              <Icon name="upload" size={18} />
              Importar cardápio
            </button>
            <button type="button" className="btn btn--ghost" onClick={() => setCategoryDialog({ mode: 'create' })}>
              Criar categoria
            </button>
          </div>
        </div>
      ) : (
        <div className="menu-layout">
          <nav className="cat-nav" aria-label="Categorias">
            <div className="cat-nav-head">Categorias</div>
            <ul className="cat-nav-list">
              {menu.categories.map((c) => (
                <li key={c.id}>
                  <button
                    type="button"
                    className="cat-nav-item"
                    aria-current={c.id === activeId ? 'true' : undefined}
                    onClick={() => setActiveId(c.id)}
                    title={c.name}
                  >
                    <span className="cat-nav-name">{c.name}</span>
                    <span className="cat-nav-count" aria-label={`${c.items.length} ${c.items.length === 1 ? 'prato' : 'pratos'}`}>
                      {c.items.length}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
            <button type="button" className="cat-nav-add" onClick={() => setCategoryDialog({ mode: 'create' })}>
              <Icon name="plus" size={16} />
              Nova categoria
            </button>
          </nav>

          <div className="cat-picker">
            <label className="sr-only" htmlFor="category-picker">
              Categoria
            </label>
            <select
              id="category-picker"
              className="select"
              value={activeId ?? ''}
              onChange={(e) => setActiveId(e.target.value)}
            >
              {menu.categories.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name} ({c.items.length})
                </option>
              ))}
            </select>
            <button
              type="button"
              className="btn btn--ghost cat-picker-add"
              onClick={() => setCategoryDialog({ mode: 'create' })}
              aria-label="Nova categoria"
            >
              <Icon name="plus" size={18} />
            </button>
          </div>

          {active && (
            <section className="menu-panel" aria-label={active.name}>
              <div className="category-bar">
                <h2>{active.name}</h2>
                <div className="page-actions">
                  <button type="button" className="btn btn--ghost btn--sm" onClick={() => setCategoryDialog({ mode: 'rename', category: active })}>
                    <Icon name="edit" size={16} />
                    Renomear
                  </button>
                  <button type="button" className="btn btn--danger btn--sm" onClick={() => setCategoryDialog({ mode: 'delete', category: active })}>
                    <Icon name="trash" size={16} />
                    Excluir
                  </button>
                </div>
              </div>

              {active.items.length === 0 ? (
                <div className="card empty">
                  <p>Nenhum prato nesta categoria ainda.</p>
                  <button type="button" className="btn btn--primary" onClick={() => setEditing({ new: true, categoryId: active.id })}>
                    <Icon name="plus" size={18} />
                    Adicionar prato
                  </button>
                </div>
              ) : (
                <div className="item-list">
                  <div className="item-list-head" aria-hidden="true">
                    <span>Prato</span>
                    <span>No chat</span>
                    <span />
                  </div>
                  {active.items.map((item) => (
                    <div key={item.id} className={`item-row ${item.isAvailable ? '' : 'is-unavailable'}`}>
                      <div className="item-main">
                        <div className="item-name">{item.name}</div>
                        <div className="item-meta">
                          <span className="item-price">{formatBRL(item.price)}</span>
                          {itemTags(item).map((tag) => (
                            <span key={tag} className={`diet-tag ${tag === 'spicy' ? 'diet-tag--spicy' : ''}`}>
                              {DIETARY_LABELS[tag]}
                            </span>
                          ))}
                        </div>
                      </div>
                      <div className="item-availability">
                        <Switch
                          checked={item.isAvailable}
                          onChange={(checked) => toggleAvailability(item, checked)}
                          label={`${item.name} disponível no chat`}
                        />
                        <span>{item.isAvailable ? 'Disponível' : 'Oculto'}</span>
                      </div>
                      <div className="item-actions">
                        <button type="button" className="icon-btn" onClick={() => setEditing(item)} aria-label={`Editar ${item.name}`}>
                          <Icon name="edit" size={18} />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </section>
          )}
        </div>
      )}

      {editing && (
        <ItemDrawer
          base={base}
          item={editing.new ? null : editing}
          defaultCategoryId={editing.categoryId ?? activeId}
          categories={menu.categories}
          onClose={() => setEditing(null)}
          onSaved={(message) => {
            setEditing(null)
            showToast(message)
            load()
          }}
        />
      )}

      {categoryDialog && (
        <CategoryDialog
          base={base}
          state={categoryDialog}
          onClose={() => setCategoryDialog(null)}
          onDone={(message, createdId) => {
            setCategoryDialog(null)
            showToast(message)
            if (createdId) setActiveId(createdId)
            load()
          }}
        />
      )}

      {importing && (
        <ImportDialog
          base={base}
          onClose={() => setImporting(false)}
          onPublished={(count) => {
            setImporting(false)
            showToast(`${count} itens publicados. Revise os atributos vegano, sem lactose e sem glúten.`)
            load()
          }}
        />
      )}
      {toast}
    </>
  )
}

function CategoryDialog({ base, state, onClose, onDone }) {
  const { mode, category } = state
  const [name, setName] = useState(category?.name ?? '')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function submit(e) {
    e?.preventDefault()
    setError('')
    if (mode !== 'delete' && !name.trim()) {
      setError('Informe o nome da categoria.')
      return
    }
    setBusy(true)
    try {
      if (mode === 'create') {
        const id = await api.post(`${base}/categories`, { name: name.trim() })
        onDone('Categoria criada.', id)
      } else if (mode === 'rename') {
        await api.put(`${base}/categories/${category.id}`, { name: name.trim() })
        onDone('Categoria renomeada.')
      } else {
        await api.del(`${base}/categories/${category.id}`)
        onDone('Categoria excluída.')
      }
    } catch (err) {
      setError(err.message)
      setBusy(false)
    }
  }

  const title = mode === 'create' ? 'Nova categoria' : mode === 'rename' ? 'Renomear categoria' : 'Excluir categoria'
  const itemCount = category?.items.length ?? 0

  return (
    <Dialog
      title={title}
      variant="modal-sm"
      onClose={onClose}
      footer={
        <>
          <button type="button" className="btn btn--ghost" onClick={onClose}>
            Cancelar
          </button>
          <button
            type="button"
            className={`btn ${mode === 'delete' ? 'btn--danger' : 'btn--primary'}`}
            onClick={submit}
            disabled={busy}
          >
            {busy ? <Spinner label="Salvando" /> : mode === 'delete' ? 'Excluir' : 'Salvar'}
          </button>
        </>
      }
    >
      <Alert>{error}</Alert>
      {mode === 'delete' ? (
        <p>
          Excluir <strong>{category.name}</strong>
          {itemCount > 0 ? ` e os ${itemCount} pratos dela` : ''}? Essa ação não pode ser desfeita.
        </p>
      ) : (
        <form onSubmit={submit}>
          <Field label="Nome" value={name} onChange={(e) => setName(e.target.value)} maxLength={80} data-autofocus />
        </form>
      )}
    </Dialog>
  )
}
