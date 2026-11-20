const MAX_SIDE = 1200
const QUALITY = 0.85

export const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp']
export const MAX_IMAGE_BYTES = 5 * 1024 * 1024

function encode(canvas, type) {
  return new Promise((resolve) => canvas.toBlob(resolve, type, QUALITY))
}

/**
 * Downscales the photo to at most 1200px on the longest side and re-encodes it (WEBP, or JPEG
 * where the browser cannot encode WEBP). Keeps uploads small on restaurant connections.
 */
export async function prepareMenuImage(file) {
  const bitmap = await createImageBitmap(file)
  const scale = Math.min(1, MAX_SIDE / Math.max(bitmap.width, bitmap.height))
  const canvas = document.createElement('canvas')
  canvas.width = Math.round(bitmap.width * scale)
  canvas.height = Math.round(bitmap.height * scale)
  const context = canvas.getContext('2d')
  context.fillStyle = '#ffffff' // transparent PNGs would turn black in JPEG
  context.fillRect(0, 0, canvas.width, canvas.height)
  context.drawImage(bitmap, 0, 0, canvas.width, canvas.height)
  bitmap.close?.()

  let blob = await encode(canvas, 'image/webp')
  if (!blob || blob.type !== 'image/webp') blob = await encode(canvas, 'image/jpeg')
  if (!blob) throw new Error('encode failed')
  return blob
}
