export interface Tenant {
  id: string
  name: string
  code: string
  isActive: boolean
  accounts?: CompanyAccount[]
}

export interface CompanyAccount {
  username: string
  email: string
  isEmailVerified: boolean
  mustChangePassword: boolean
}

export interface UpsertTenantRequest {
  name: string
  code: string
  accountEmail?: string
  accountPassword?: string
}
