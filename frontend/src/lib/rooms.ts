import api from '@/lib/api';

export interface RoomImageDto {
  id: number;
  roomId: number;
  imageUrl: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface RoomDto {
  id: number;
  ownerId: string;
  title: string;
  description: string;
  price: number;
  deposit?: number | null;
  area: number;
  maxOccupants: number;
  address: string;
  amenities?: string | null;
  rules?: string | null;
  status: string;
  isFeatured: boolean;
  isVIP: boolean;
  viewCount: number;
  createdAt: string;
}

export interface RoomOwnerDto {
  id: string;
  fullName?: string | null;
  avatarUrl?: string | null;
  isIdVerified?: boolean;
  totalListings?: number;
}

export interface RoomWithDetails {
  room: RoomDto;
  images: RoomImageDto[];
  owner: RoomOwnerDto | null;
}

/**
 * Resolve a room image URL coming from the API.
 * Absolute URLs are kept as-is; relative paths like "/images/rooms/x.jpg"
 * are resolved against the API base URL (the backend serves the files).
 */
export function resolveImageUrl(url: string | null | undefined): string | null {
  if (!url) return null;
  if (/^https?:\/\//i.test(url)) return url;
  const base = (process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5246').replace(/\/api\/?$/, '');
  return `${base}${url.startsWith('/') ? '' : '/'}${url}`;
}

export interface ApiRoomFilter {
  location?: string;
  price?: string;
}

export async function fetchRooms(filter: ApiRoomFilter = {}): Promise<RoomWithDetails[]> {
  const params: Record<string, string> = {};
  if (filter.location) params.location = filter.location;
  if (filter.price) params.price = filter.price;

  const res = await api.get<RoomWithDetails[]>('/rooms', { params });
  return Array.isArray(res.data) ? res.data : [];
}

export function formatPrice(price: number): string {
  return new Intl.NumberFormat('vi-VN').format(price) + 'đ';
}

export function formatArea(area: number): string {
  return `${new Intl.NumberFormat('vi-VN').format(area)} m²`;
}
