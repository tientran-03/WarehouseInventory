export type WarehouseLayoutType = 'cross_dock_standard' | 'u_shaped_flow' | 'high_density_racking' | 'ecommerce_fulfillment';

// ── Zone-based layout types ──

export type LayoutZoneType =
  | 'dock-in'     // Bến nhận hàng
  | 'dock-out'    // Bến xuất hàng
  | 'receiving'   // Khu nhận hàng staging
  | 'qa'          // Khu kiểm hàng QA
  | 'rack'        // Hệ thống kệ lưu kho
  | 'picking'     // Khu nhặt hàng
  | 'conveyor'    // Băng chuyền
  | 'packaging'   // Khu đóng gói
  | 'shipping'    // Khu xuất hàng staging
  | 'office'      // Văn phòng
  | 'lounge'      // Phòng nghỉ
  | 'custom';     // Zone tùy chỉnh

export interface LayoutZoneMeta {
  dockCount?: number;
  rackRows?: string[];
  binsPerRow?: number;
  tableCount?: number;
  [key: string]: any;
}

export interface LayoutZone {
  id: string;
  type: LayoutZoneType;
  name: string;
  x: number;       // vị trí X trong SVG viewBox (0–1100)
  y: number;       // vị trí Y trong SVG viewBox (0–650)
  width: number;   // chiều rộng (SVG units)
  height: number;  // chiều cao  (SVG units)
  color?: string;  // màu nền tuỳ chỉnh
  meta?: LayoutZoneMeta;
}

// ── Warehouse entity ──

export interface Warehouse {
  id: string
  tenantId: string
  code: string
  name: string
  address: string
  city: string
  latitude: number
  longitude: number
  isActive: boolean
  layoutType?: WarehouseLayoutType
  layoutConfigJson?: string
}

export interface UpsertWarehouseRequest {
  tenantId: string
  code: string
  name: string
  address: string
  city: string
  latitude: number
  longitude: number
  layoutType?: WarehouseLayoutType
  layoutConfigJson?: string
}
