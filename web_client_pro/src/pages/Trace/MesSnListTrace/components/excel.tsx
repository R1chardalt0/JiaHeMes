import React from 'react';
import { Button } from 'antd';
import { DownloadOutlined } from '@ant-design/icons';
import * as XLSX from 'xlsx';
import { message } from 'antd';

/** 
 * Excel导出配置接口 
 */
export interface ExcelExportConfig {
  /** 信息行：二维字符串数组，如 [['导出时间: 2025-10-21'], ['搜索文本: 无']] */
  infoRows?: string[][];
  /** 表头 */
  headers: string[];
  /** 数据行：二维字符串数组 */
  dataRows: string[][];
  /** 文件名（不含扩展名） */
  filename: string;
  /** 列宽配置（可选） */
  columnWidths?: number[];
}

/** 
 * 通用Excel导出函数 
 * @param config 导出配置 
 */
export const exportToExcel = (config: ExcelExportConfig): void => {
  const { infoRows = [], headers, dataRows, filename, columnWidths } = config;

  try {
    // 组合数据：信息行 + 表头 + 数据行
    const wsData = [...infoRows, headers, ...dataRows];

    // 创建工作表
    const ws = XLSX.utils.aoa_to_sheet(wsData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');

    // 设置列宽
    if (columnWidths && columnWidths.length > 0) {
      ws['!cols'] = columnWidths.map(wch => ({ wch }));
    } else {
      // 自动计算列宽（考虑表头和数据）
      const colWidths = headers.map((_, colIndex) => {
        const allValues = [
          ...infoRows.map(row => row[colIndex] || ''),
          headers[colIndex],
          ...dataRows.map(row => row[colIndex] || '')
        ];
        const maxWidth = Math.max(
          ...allValues.map(val =>
            typeof val === 'string' ? val.length : 0
          ),
          10 // 最小宽度
        );
        return { wch: Math.min(maxWidth + 2, 50) }; // 限制最大宽度
      });
      ws['!cols'] = colWidths;
    }

    XLSX.writeFile(wb, `${filename}.xlsx`);
  } catch (error) {
    console.error('Excel导出失败:', error);
    message.error('导出Excel文件失败');
  }
};

/** 
 * 辅助函数：将对象数组转为字符串二维数组 
 * @param data 对象数组 
 * @param columns 列配置 
 * @returns 字符串二维数组 
 */
export const convertObjectsToRows = <T extends Record<string, any>>(
  data: T[],
  columns: { key: string; title: string; formatter?: (value: any) => string }[]
): string[][] => {
  if (!Array.isArray(data)) return [];

  return data.map(item =>
    columns.map(col => {
      const value = item[col.key];
      if (value == null) return '';
      if (col.formatter) {
        try {
          return col.formatter(value);
        } catch (e) {
          console.error('格式化错误:', e);
          return String(value);
        }
      }
      if (typeof value === 'object') return JSON.stringify(value);
      return String(value);
    })
  );
};

/** 
 * 辅助函数：创建信息行 
 * @param exportTime 导出时间 
 * @param searchParams 搜索参数信息 
 * @param totalRecords 总记录数 
 * @returns 信息行二维数组 
 */
export const createInfoRows = (
  exportTime: string,
  searchParams: Record<string, any>,
  totalRecords: number
): string[][] => {
  const infoRows = [
    [`导出时间: ${exportTime}`],
    [''] // 空行
  ];

  // 添加搜索条件信息
  Object.entries(searchParams).forEach(([key, value]) => {
    // 跳过分页参数
    if (key === 'pageIndex' || key === 'pageSize' || key === 'current') return;

    // 跳过空值
    if (value === undefined || value === null) return;

    let displayValue: string;

    // 处理不同类型的值
    if (Array.isArray(value)) {
      // 处理日期范围等数组类型
      displayValue = value.join(' 至 ');
    } else if (typeof value === 'boolean') {
      // 处理布尔值
      displayValue = value ? '是' : '否';
    } else if (typeof value === 'object') {
      // 处理对象类型
      displayValue = JSON.stringify(value);
    } else {
      // 其他类型直接转换为字符串
      displayValue = String(value);
    }

    // 添加到信息行
    infoRows.push([`${key}: ${displayValue}`]);
  });

  infoRows.push(['']); // 空行
  infoRows.push([`总记录数: ${totalRecords}条`]);
  infoRows.push(['']); // 空行

  return infoRows;
};

/**
 * Excel导出按钮组件
 */
interface ExcelExportButtonProps {
  /** 要导出的数据 */
  data: any[];
  /** 列配置 */
  columns: any[];
  /** 导出文件名 */
  filename: string;
  /** 搜索参数 */
  searchParams?: Record<string, string>;
  /** 按钮文本，默认"导出Excel" */
  buttonText?: string;
  /** 按钮样式 */
  style?: React.CSSProperties;
}

export const ExcelExportButton: React.FC<ExcelExportButtonProps> = ({
  data,
  columns,
  filename,
  searchParams = {},
  buttonText = '导出Excel',
  style = {}
}) => {
  const handleExport = () => {
    try {
      // 过滤掉需要导出的列（排除隐藏列、没有dataIndex的列和操作列）
      const exportColumns = columns
        .filter(col => col.dataIndex && col.title && col.key !== 'operation' && col.hideInTable !== true)
        .map(col => ({
          key: col.dataIndex,
          title: col.title,
          formatter: col.render
        }));

      // 转换数据
      const dataRows = convertObjectsToRows(data, exportColumns);

      // 创建信息行
      const exportTime = new Date().toLocaleString();
      const infoRows = createInfoRows(exportTime, searchParams, data.length);

      // 导出配置
      const config: ExcelExportConfig = {
        infoRows,
        headers: exportColumns.map(col => col.title),
        dataRows,
        filename: `${filename}_${exportTime.replace(/[\s:]/g, '-')}`
      };

      // 执行导出
      exportToExcel(config);
      message.success('导出成功');
    } catch (error) {
      console.error('导出失败:', error);
      message.error('导出失败，请稍后重试');
    }
  };

  return (
    <Button
      type="primary"
      icon={<DownloadOutlined />}
      onClick={handleExport}
      style={{ marginBottom: 16, ...style }}
      disabled={data.length === 0}
    >
      {buttonText}
    </Button>
  );
};

export default ExcelExportButton;