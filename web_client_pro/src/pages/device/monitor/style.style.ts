import { createStyles } from 'antd-style';

const useStyles = createStyles(({ token }) => {
  return {
    card: {
      position: 'relative',
      paddingBottom: '0px', // 给底部标题留出空间
      // 将 meta 标题固定到卡片底部并居中
      '.ant-card-meta-title': {
        background: 'radial-gradient(circle farthest-side at 10% 50%, rgba(38, 46, 90, 0.65), rgba(0, 10, 50, 1)70%)',
        // boxShadow: '2px -5px 10px rgba(0, 10, 50, 0.75)',
        fontSize: '28px',
        // letterSpacing: '5px',
        padding: '23px 0 25px 30px',
        margin: '185px 0',
        position: 'absolute',
        left: 0,
        right: 0,
        // bottom: 55,
        textAlign: 'left',
        textShadow: '3px 3px 5px rgba(0, 0, 0, 0.45)',
        width: '100%',
        zIndex: 2,
        '& > a': {
          display: 'inline-block',
          maxWidth: '100%',
          color: token.colorTextHeading,
        },
      }, 
      // 保留原有 hover 高亮逻辑（会作用在绝对定位的标题上）
      '.ant-card-body:hover': {
        '.ant-card-meta-title > a': {
          color: 'rgba(255, 255, 255, 1)',
          textShadow: '3px 3px 5px rgba(0, 0, 0, 0.9)',
        },
      },
      // 统一操作按钮为深色半透明玻璃风
      '.ant-card-actions': {
        // borderTop: `0px solid rgba(255,255,255,0)`,
        // background: 'linear-gradient(rgb(190, 255, 255), rgb(40,185,150)50%)',
        '& > li': {
          padding: '0px 0px',
        },
        '& > li > span, & > li > a': {
          display: 'inline-flex',
          alignItems: 'center',
          // justifyContent: 'center',
          minWidth: '68px',
          height: '25px',
          zIndex: '100',
          margin: '-95px 0px 10px 250px',
          // padding: '0 14px',
          // color: '#E6F7FF',
          background: 'linear-gradient(145deg, rgba(65, 255, 230, 1), rgba(24, 144, 255, 1)95%)',
          // background: 'linear-gradient( rgba(24, 144, 255, 1), rgba(190, 229, 255, 1)95%)',
          // border: '2px solid rgba(118, 201, 246, 1)',
          borderRadius: 20,
          boxShadow: '0 4px 14px rgba(0,0,0,0) inset, 0 6px 16px rgba(0,0,0,0.25)',
          textDecoration: 'none',
          transition: 'all 0.2s ease',
          fontSize: '12px',
        },
        '& > li > span:hover, & > li > a:hover': {
          // borderColor: 'rgba(65, 255, 235, 1)',
          // // boxShadow: '0 0 0 2px rgba(25,225,255,0.15), 0 6px 16px rgba(0,0,0,0.3)',
          // color: 'rgba(255, 255, 255, 1)',
        },
        '& > li > span:active, & > li > a:active': {
          transform: 'translateY(1px)',
        },
      },
    },
    item: {
      height: '64px',
    },
    cardList: {
      '.ant-list .ant-list-item-content-single': { maxWidth: '100%' },
    },
    extraImg: {
      width: '155px',
      marginTop: '-20px',
      textAlign: 'center',
      img: { width: '100%' },
      [`@media screen and (max-width: ${token.screenMD}px)`]: {
        display: 'none',
      },
    },
    newButton: {
      width: '100%',
      height: '201px',
      color: token.colorTextSecondary,
      backgroundColor: token.colorBgContainer,
      borderColor: token.colorBorder,
    },
    cardAvatar: {
      width: '48px',
      height: '48px',
      borderRadius: '48px',
    },
    cardAvatarPlaceholder: {
      display: 'inline-block',
      width: '48px',
      height: '48px',
      borderRadius: '48px',
      backgroundColor: token.colorFillSecondary,
      border: `1px solid ${token.colorBorder}`,
    },
    typeCard: {
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'space-between',
      alignItems: 'center',
      padding: '16px 16px 0',
      '.ant-card-actions': {
        borderTop: '0px solid rgba(255,255,255,0)',
        background: 'transparent',
        '& > li': {
          // padding: '0px 0px',
        },
        '& > li > span, & > li > a': {
          // display: 'inline-flex',
          // alignItems: 'center',
          // justifyContent: 'center',
          // // minWidth: '90px',
          // // height: '30px',
          // zIndex: 100,
          // // padding: '0 14px',
          // // color: '#E6F7FF',
          // // background: 'linear-gradient( rgba(24, 144, 255, 1), rgba(190, 229, 255, 1)95%)',
          // // border: '2px solid rgba(118, 201, 246, 1)',
          // // borderRadius: 20,
          // // boxShadow: '0 0 8px rgba(0, 255, 119, 1)',
          // // textDecoration: 'none',
          // // transition: 'all 0.2s ease',
          // fontSize: 14,
        },
        '& > li > span:hover, & > li > a:hover': {
          background: ' linear-gradient(rgba(130, 250, 175, 1), rgba(145, 250, 240, 1))',
          border: '2px solid rgba(65, 255, 230, 1)',
          boxShadow: '0 0 8px rgba(65, 255, 230, 1)',
          color: 'rgba(255, 255, 255, 1)',
        },
        '& > li > span:active, & > li > a:active': {
          transform: 'translateY(1px)',
        },
      },
      // 悬停时标题高亮
      ':hover .typeName': {
        color: 'rgb(65, 255, 235)',
      },
      // 与设备列表卡片 hover 一致
      '.ant-card-body:hover .ant-card-meta-title > a': {
        color: 'rgba(255, 255, 255, 1)',
      },
    },
    typeCardContent: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 12,
      justifyContent: 'center',
      minHeight: '201px', // 与设备列表模块高度一致
      width: '100%',
      textAlign: 'center',
    },
    typeName: {
      margin: 0,
      fontWeight: 600,
      color: token.colorTextHeading,
      textAlign: 'center',
      transition: 'color 0.2s ease',
    },
    typeAction: {
      display: 'block',
      width: '100%',
      textAlign: 'center',
      color: token.colorTextSecondary,
      fontSize: 14,
      padding: '8px 0',
    },
    cardDescription: {
      overflow: 'hidden',
      whiteSpace: 'nowrap',
      textOverflow: 'ellipsis',
      wordBreak: 'break-all',
    },
    pageHeaderContent: {
      position: 'relative',
      [`@media screen and (max-width: ${token.screenSM}px)`]: {
        paddingBottom: '30px',
      },
    },
    contentLink: {
      marginTop: '16px',
      a: {
        marginRight: '32px',
        img: {
          width: '24px',
        },
      },
      img: { marginRight: '8px', verticalAlign: 'middle' },
      [`@media screen and (max-width: ${token.screenLG}px)`]: {
        a: {
          marginRight: '16px',
        },
      },
      [`@media screen and (max-width: ${token.screenSM}px)`]: {
        position: 'absolute',
        bottom: '-4px',
        left: '0',
        width: '1000px',
        a: {
          marginRight: '16px',
        },
        img: {
          marginRight: '4px',
        },
      },
    },
  };
});

export default useStyles;
