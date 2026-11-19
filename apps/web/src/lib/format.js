const brl = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

export const formatBRL = (value) => brl.format(Number(value) || 0)

/** "38,90" / "38.90" / "38" → 38.9 (null when invalid). */
export function parsePrice(text) {
  const normalized = String(text ?? '')
    .trim()
    .replace(/[^\d,.-]/g, '')
    .replace(/\.(?=\d{3}(\D|$))/g, '')
    .replace(',', '.')
  if (normalized === '') return null
  const value = Number(normalized)
  return Number.isFinite(value) && value >= 0 ? Math.round(value * 100) / 100 : null
}

export const priceInput = (value) =>
  value === null || value === undefined || value === '' ? '' : Number(value).toFixed(2).replace('.', ',')

export const DIETARY_LABELS = {
  vegan: 'Vegano',
  lactose_free: 'Sem lactose',
  gluten_free: 'Sem glúten',
  spicy: 'Apimentado',
}

export function itemTags(item) {
  const tags = []
  if (item.isVegan) tags.push('vegan')
  if (item.isLactoseFree) tags.push('lactose_free')
  if (item.isGlutenFree) tags.push('gluten_free')
  if (item.isSpicy) tags.push('spicy')
  return tags
}

export function greetingForNow(date = new Date()) {
  const hour = date.getHours()
  if (hour >= 5 && hour < 12) return 'Bom dia'
  if (hour >= 12 && hour < 18) return 'Boa tarde'
  return 'Boa noite'
}

export const chatUrl = (code) => `${window.location.origin}/m/${code}`
