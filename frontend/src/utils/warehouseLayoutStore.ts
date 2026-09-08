import type { LayoutZone, LayoutZoneType, WarehouseLayoutType } from '../types/warehouse.types';

// ── Layout config interface (zone-based) ──

export interface WarehouseLayoutConfig {
  warehouseId: string;
  isCustom: boolean;
  name?: string;
  zones: LayoutZone[];
}

// ── Zone type metadata for palette ──

export interface ZoneTypeInfo {
  type: LayoutZoneType;
  label: string;
  icon: string;
  defaultWidth: number;
  defaultHeight: number;
  color: string;
  defaultMeta?: Record<string, any>;
}

export const ZONE_TYPE_INFO: ZoneTypeInfo[] = [
  { type: 'dock-in',    label: 'Bến nhận hàng',     icon: '', defaultWidth: 120, defaultHeight: 200, color: '#dbeafe', defaultMeta: { dockCount: 3 } },
  { type: 'dock-out',   label: 'Bến xuất hàng',     icon: '', defaultWidth: 120, defaultHeight: 200, color: '#dbeafe', defaultMeta: { dockCount: 3 } },
  { type: 'receiving',  label: 'Khu nhận hàng',     icon: '', defaultWidth: 110, defaultHeight: 195, color: '#f1f5f9' },
  { type: 'qa',         label: 'Khu kiểm hàng QA',  icon: '', defaultWidth: 200, defaultHeight: 195, color: '#fef3c7' },
  { type: 'rack',       label: 'Khu vực kệ lưu kho', icon: '', defaultWidth: 455, defaultHeight: 365, color: '#f0fdf4', defaultMeta: { rackRows: ['A', 'B', 'C', 'D', 'E'], binsPerRow: 10 } },
  { type: 'picking',    label: 'Khu nhặt hàng',     icon: '', defaultWidth: 250, defaultHeight: 150, color: '#ede9fe' },
  { type: 'conveyor',   label: 'Băng chuyền',       icon: '', defaultWidth: 200, defaultHeight: 60,  color: '#fef3c7' },
  { type: 'packaging',  label: 'Khu đóng gói',      icon: '', defaultWidth: 200, defaultHeight: 195, color: '#fff7ed' },
  { type: 'shipping',   label: 'Khu xuất hàng',     icon: '', defaultWidth: 120, defaultHeight: 195, color: '#f1f5f9' },
  { type: 'office',     label: 'Văn phòng kho',     icon: '', defaultWidth: 355, defaultHeight: 195, color: '#fafafa' },
  { type: 'lounge',     label: 'Phòng nghỉ Lounge', icon: '', defaultWidth: 110, defaultHeight: 155, color: '#fafafa' },
  { type: 'custom',     label: 'Khu vực tùy chỉnh', icon: '', defaultWidth: 150, defaultHeight: 120, color: '#f8fafc' },
];

// ── Default Cross-Dock zones matching the CAD blueprint ──

export const DEFAULT_CROSS_DOCK_ZONES: LayoutZone[] = [
  {
    id: 'dock-in',
    type: 'dock-in',
    name: 'Bến nhận hàng (Inbound Docks)',
    x: 120, y: 25, width: 125, height: 210,
    color: '#dbeafe',
    meta: { dockCount: 3 },
  },
  {
    id: 'lounge',
    type: 'lounge',
    name: 'Phòng nghỉ & Tiếp khách (Lounge)',
    x: 120, y: 240, width: 115, height: 155,
    color: '#fafafa',
  },
  {
    id: 'dock-out',
    type: 'dock-out',
    name: 'Bến xuất hàng (Outbound Docks)',
    x: 120, y: 400, width: 125, height: 220,
    color: '#dbeafe',
    meta: { dockCount: 3 },
  },
  {
    id: 'receiving',
    type: 'receiving',
    name: 'Khu vực nhận hàng (Receiving)',
    x: 245, y: 25, width: 115, height: 200,
    color: '#f1f5f9',
  },
  {
    id: 'qa',
    type: 'qa',
    name: 'Khu kiểm hàng (QA & Inspection)',
    x: 365, y: 25, width: 205, height: 200,
    color: '#fef3c7',
  },
  {
    id: 'picking',
    type: 'picking',
    name: 'Khu nhặt hàng (Order Picking)',
    x: 315, y: 240, width: 255, height: 155,
    color: '#ede9fe',
  },
  {
    id: 'rack',
    type: 'rack',
    name: 'Hệ thống kệ lưu kho',
    x: 585, y: 25, width: 460, height: 370,
    color: '#f0fdf4',
    meta: { rackRows: ['A', 'B', 'C', 'D', 'E'], binsPerRow: 10 },
  },
  {
    id: 'shipping',
    type: 'shipping',
    name: 'Khu xuất hàng (Outbound Staging)',
    x: 245, y: 400, width: 125, height: 220,
    color: '#f1f5f9',
  },
  {
    id: 'packaging',
    type: 'packaging',
    name: 'Khu đóng gói & Băng chuyền',
    x: 475, y: 400, width: 205, height: 220,
    color: '#fff7ed',
    meta: { tableCount: 8 },
  },
  {
    id: 'office',
    type: 'office',
    name: 'Văn phòng điều hành kho',
    x: 685, y: 400, width: 360, height: 220,
    color: '#fafafa',
  },
];

const STORAGE_KEY_PREFIX = 'mwi_warehouse_layout_v2_';

export const DEFAULT_LAYOUT_CONFIG: WarehouseLayoutConfig = {
  warehouseId: 'default',
  isCustom: false,
  name: 'Kho Cross-Docking Tiêu chuẩn',
  zones: DEFAULT_CROSS_DOCK_ZONES,
};

export function emptyWarehouseLayout(warehouseId: string, warehouseName?: string): WarehouseLayoutConfig {
  return {
    warehouseId,
    isCustom: false,
    name: warehouseName ? `Sơ đồ ${warehouseName}` : 'Sơ đồ trống',
    zones: [],
  };
}

export function hasCustomWarehouseLayout(warehouseId?: string): boolean {
  if (!warehouseId) return false;
  try {
    return Boolean(localStorage.getItem(`${STORAGE_KEY_PREFIX}${warehouseId}`));
  } catch {
    return false;
  }
}

export function getWarehouseLayoutConfig(warehouseId?: string, warehouseName?: string): WarehouseLayoutConfig {
  if (!warehouseId) return emptyWarehouseLayout('default', warehouseName);
  try {
    const saved = localStorage.getItem(`${STORAGE_KEY_PREFIX}${warehouseId}`);
    if (saved) {
      const parsed = JSON.parse(saved) as WarehouseLayoutConfig;
      if (Array.isArray(parsed.zones)) {
        return { ...parsed, warehouseId };
      }
    }
  } catch (err) {
    console.error('Failed to read warehouse layout from localStorage', err);
  }
  return emptyWarehouseLayout(warehouseId, warehouseName);
}

export function saveWarehouseLayoutConfig(config: WarehouseLayoutConfig): void {
  if (!config.warehouseId) return;
  try {
    localStorage.setItem(
      `${STORAGE_KEY_PREFIX}${config.warehouseId}`,
      JSON.stringify({ ...config, isCustom: true })
    );
  } catch (err) {
    console.error('Failed to save warehouse layout to localStorage', err);
  }
}

export function resetWarehouseToDefault(warehouseId: string): void {
  if (!warehouseId) return;
  try {
    localStorage.removeItem(`${STORAGE_KEY_PREFIX}${warehouseId}`);
  } catch (err) {
    console.error('Failed to reset warehouse layout', err);
  }
}

// ── Zone CRUD helpers ──

let _idCounter = 0;

export function createZoneId(type: LayoutZoneType): string {
  _idCounter++;
  return `${type}-${Date.now()}-${_idCounter}`;
}

export function addZone(config: WarehouseLayoutConfig, zone: LayoutZone): WarehouseLayoutConfig {
  return { ...config, zones: [...config.zones, zone], isCustom: true };
}

export function updateZone(config: WarehouseLayoutConfig, zoneId: string, updates: Partial<LayoutZone>): WarehouseLayoutConfig {
  return {
    ...config,
    isCustom: true,
    zones: config.zones.map(z => z.id === zoneId ? { ...z, ...updates } : z),
  };
}

export function removeZone(config: WarehouseLayoutConfig, zoneId: string): WarehouseLayoutConfig {
  return {
    ...config,
    isCustom: true,
    zones: config.zones.filter(z => z.id !== zoneId),
  };
}

export function duplicateZone(config: WarehouseLayoutConfig, zoneId: string): WarehouseLayoutConfig {
  const source = config.zones.find(z => z.id === zoneId);
  if (!source) return config;
  const newZone: LayoutZone = {
    ...source,
    id: createZoneId(source.type),
    name: `${source.name} (Bản sao)`,
    x: source.x + 30,
    y: source.y + 30,
  };
  return addZone(config, newZone);
}

export const LAYOUT_TEMPLATES: {
  id: WarehouseLayoutType;
  name: string;
  icon: string;
  tag: string;
  totalRacks: number;
}[] = [
  {
    id: 'cross_dock_standard',
    name: 'Cross‑Dock tiêu chuẩn',
    icon: '',
    tag: 'Chuẩn CAD',
    totalRacks: 50,
  },
];

export function setWarehouseLayoutType(warehouseId: string, _layoutType: WarehouseLayoutType): void {
  if (!warehouseId) return;
}