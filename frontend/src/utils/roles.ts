export function isPlatformAdmin(role?: string | null) {
  return (role ?? '').toLowerCase() === 'admin'
}

export function homePathForRole(role?: string | null) {
  return isPlatformAdmin(role) ? '/tenants' : '/'
}
