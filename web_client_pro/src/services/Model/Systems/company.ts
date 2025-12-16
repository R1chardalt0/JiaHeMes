export interface SysCompany {
  companyId: number;
  companyName: string;
  companyCode: string;
  createTime?: string;
  updateTime?: string;
  remark?: string;
}

export interface CompanyItem extends SysCompany {}

export interface CompanyQueryParams {
  current?: number;
  pageSize?: number;
  companyName?: string;
}

export interface CompanyPagedResp {
  success: boolean;
  data: CompanyItem[];
  total: number;
  page: number;
}

