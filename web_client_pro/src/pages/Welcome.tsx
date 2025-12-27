import { PageContainer } from '@ant-design/pro-components';
import { useModel } from '@umijs/max';
import { Card, theme } from 'antd';
import React from 'react';

/**
 * 每个单独的卡片，为了复用样式抽成了组件
 * @param param0
 * @returns
 */
const InfoCard: React.FC<{
  title: string;
  index: number;
  desc: string;
  href: string;
}> = ({ title, href, index, desc }) => {
  const { useToken } = theme;

  const { token } = useToken();

  return (
    <div
      style={{
        ...(window as any).__panelStyles?.panelStyle,
        borderRadius: 12,
        fontSize: '14px',
        color: '#E6F7FF',
        lineHeight: '22px',
        padding: '16px 19px',
        minWidth: '220px',
        flex: 1,
        boxShadow: '0 8px 24px rgba(0,0,0,0.25)',
      }}
    >
      <div
        style={{
          display: 'flex',
          gap: '4px',
          alignItems: 'center',
        }}
      >
        <div
          style={{
            width: 48,
            height: 48,
            lineHeight: '22px',
            backgroundSize: '100%',
            textAlign: 'center',
            padding: '8px 16px 16px 12px',
            color: '#E6F7FF',
            fontWeight: 'bold',
            backgroundImage:
              "url('https://gw.alipayobjects.com/zos/bmw-prod/daaf8d50-8e6d-4251-905d-676a24ddfa12.svg')",
          }}
        >
          {index}
        </div>
        <div
          style={{
            fontSize: '16px',
            color: token.colorText,
            paddingBottom: 8,
          }}
        >
          {title}
        </div>
      </div>
      <div
        style={{
          fontSize: '14px',
          color: 'rgba(230,247,255,0.85)',
          textAlign: 'justify',
          lineHeight: '20px',
          marginBottom: 4,
        }}
      >
        {desc}
      </div>
      <a
        href={href}
        target="_blank"
        rel="noreferrer"
        style={{
          color: '#91d5ff',
          fontWeight: 500,
          textDecoration: 'none',
        }}
      >
        了解更多 {'>'}
      </a>
    </div>
  );
};

const Welcome: React.FC = () => {
  const { initialState } = useModel('@@initialState');
  React.useEffect(() => {
    const prev = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = prev;
    };
  }, []);
  return (
    <PageContainer header={{ title: '欢迎使用MES管理系统' }} fixedHeader>
      {/** 注入统一面板样式常量 */}
      {/** @ts-ignore */}
      {(() => {
        const anyWin: any = window as any;
        if (!anyWin.__panelStyles) {
          anyWin.__panelStyles = {
            panelStyle: {
              background: 'linear-gradient(180deg, rgba(7,16,35,0.65) 0%, rgba(7,16,35,0.35) 100%)',
              border: '1px solid rgba(255,255,255,0.12)',
              borderRadius: 12,
              boxShadow: '0 10px 30px rgba(0,0,0,0.25)',
              backdropFilter: 'blur(6px)',
              WebkitBackdropFilter: 'blur(6px)',
              overflow: 'hidden',
            },
            headStyle: {
              background: 'transparent',
              color: '#E6F7FF',
              borderBottom: '1px solid rgba(255,255,255,0.08)',
              fontWeight: 600,
            },
            bodyStyle: {
              background: 'transparent',
              padding: 16,
              color: '#E6F7FF',
            },
          };
        }
        return null;
      })()}
      
      {/* 官网内容嵌入区域 */}
      <Card
        style={{ ...(window as any).__panelStyles?.panelStyle, borderRadius: 12, marginTop: 8, marginBottom: 0 }}
        headStyle={(window as any).__panelStyles?.headStyle}
        bodyStyle={{ ...(window as any).__panelStyles?.bodyStyle, padding: 4 }}
        bordered={false}
      >
        <div
          style={{
            width: '100%',
            height: 'calc(100vh - 180px)',
            overflow: 'hidden',
            borderRadius: 8,
          }}
        >
          <iframe
            src=""
            title="江苏精研科技官网"
            width="100%"
            height="100%"
            frameBorder="0"
            style={{
              border: 'none',
              overflow: 'auto',
              width: '100%',
              height: '100%',
            }}
            scrolling="yes"
            allowFullScreen={true}
          />
        </div>
      </Card>
    </PageContainer>
  );
};

export default Welcome;
