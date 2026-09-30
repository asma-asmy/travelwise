export type DocumentType =
  | 'Passport' | 'Visa' | 'Insurance' | 'Vaccination'
  | 'Permit' | 'NationalId' | 'DriverLicense'

export interface TravelDocument {
  id: string
  userId: string
  type: DocumentType
  typeName: string
  number: string
  issuedDate: string | null
  expiryDate: string | null
  fileRef: string | null
  notes: string | null
  isExpired: boolean
  daysUntilExpiry: number | null
  createdAt: string
  updatedAt: string
}

export interface DocumentRequirement {
  id: string
  destination: string
  tripType: string
  tripTypeName: string
  documentType: DocumentType
  documentTypeName: string
  isMandatory: boolean
  notes: string | null
}

export interface ReadinessAssessment {
  id: string
  tripId: string
  score: number
  assessedAt: string
  missingDocuments: MissingDocument[]
  expiringDocuments: ExpiringDocument[]
  blockingIssues: string[]
  warnings: string[]
}

export interface MissingDocument {
  documentTypeName: string
  isMandatory: boolean
  notes: string | null
}

export interface ExpiringDocument {
  documentId: string
  documentTypeName: string
  expiryDate: string
  daysUntilExpiry: number
  expiresDuringTrip: boolean
}

export interface DocumentQuery {
  type?: DocumentType
  expiredOnly?: boolean
  expiringWithinDays?: boolean
  sortBy?: 'ExpiryDate' | 'Type'
  sortDir?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export interface CreateDocumentRequest {
  type: DocumentType
  number: string
  issuedDate?: string | null
  expiryDate?: string | null
  fileRef?: string | null
  notes?: string | null
}

export interface UpdateDocumentRequest {
  type: DocumentType
  number: string
  issuedDate?: string | null
  expiryDate?: string | null
  fileRef?: string | null
  notes?: string | null
}

export interface CreateDocumentRequirementRequest {
  destination: string
  tripType: number
  documentType: number
  isMandatory: boolean
  notes?: string | null
}