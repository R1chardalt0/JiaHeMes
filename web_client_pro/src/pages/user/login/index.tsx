import { LockOutlined, UserOutlined } from '@ant-design/icons';
import { LoginForm, ProFormText, ProFormInstance } from '@ant-design/pro-components';
import {
  FormattedMessage,
  Helmet,
  SelectLang,
  useIntl,
  useModel,
} from '@umijs/max';
import { Alert, App, Checkbox, Button } from 'antd';
import { createStyles } from 'antd-style';
import React, { useState, useEffect, useRef } from 'react';
import { flushSync } from 'react-dom';
import { Footer } from '@/components';
import { login } from '@/services/Api/Systems/login';
import { LoginResponse, UserLoginDto } from '@/services/Model/Systems/Login';
import { setAuthCookie, setTokenCookie, setCookie, COOKIE_KEYS } from '@/utils/cookieUtils';
import {
  saveUserAccount,
  saveUserPassword,
  clearUserPassword,
  getUserAccount,
  getUserPassword,
} from '@/utils/rememberPasswordUtils';
import RegisterModal from './RegisterModal';

// import { getFakeCaptcha } from '@/services/Api/Systems/login';
import Settings from '../../../../config/defaultSettings';

const useStyles = createStyles(({ token }) => {
  return {
    action: {
      marginLeft: '8px',
      color: 'rgba(0, 0, 0, 0.2)',
      fontSize: '24px',
      verticalAlign: 'middle',
      cursor: 'pointer',
      transition: 'color 0.3s',
      '&:hover': {
        color: token.colorPrimaryActive,
      },
    },
    lang: {
      width: 42,
      height: 42,
      lineHeight: '42px',
      position: 'fixed',
      right: 16,
      borderRadius: token.borderRadius,
      ':hover': {
        backgroundColor: token.colorBgTextHover,
      },
    },
    container: {
      display: 'flex',
      flexDirection: 'column',
      height: '100vh',
      overflow: 'auto',
      backgroundImage: `url(${(window as any)?.publicPath || '/'}Loginbackgrounds4.svg)`,
      backgroundRepeat: 'no-repeat',
      backgroundPosition: 'center',
      backgroundSize: 'cover',
    },
  };
});

// const ActionIcons = () => {
//   const { styles } = useStyles();
//   return (
//     <>
//       <AlipayCircleOutlined key="AlipayCircleOutlined" className={styles.action} />
//       <TaobaoCircleOutlined key="TaobaoCircleOutlined" className={styles.action} />
//       <WeiboCircleOutlined key="WeiboCircleOutlined" className={styles.action} />
//     </>
//   );
// };

const Lang = () => {
  const { styles } = useStyles();

  return (
    <div className={styles.lang} data-lang>
      {SelectLang && <SelectLang />}
    </div>
  );
};

const LoginMessage: React.FC<{
  content: string;
}> = ({ content }) => {
  return (
    <Alert
      style={{
        marginBottom: 24,
      }}
      message={content}
      type="error"
      showIcon
    />
  );
};

const Login: React.FC = () => {
  const [userLoginState, setUserLoginState] = useState<API.LoginResult>({});
  // const [type, setType] = useState<string>('account');
  const { initialState, setInitialState } = useModel('@@initialState');
  const { styles } = useStyles();
  const { message } = App.useApp();
  const intl = useIntl();
  const [rememberPassword, setRememberPassword] = useState(false);
  const [initialFormValues, setInitialFormValues] = useState<{ userName?: string; passWord?: string }>({});
  const [registerOpen, setRegisterOpen] = useState(false);
  const formRef = useRef<ProFormInstance<any>>(null);

  const fetchUserInfo = async () => {
    const userInfo = await initialState?.fetchUserInfo?.();
    if (userInfo) {
      flushSync(() => {
        setInitialState((s) => ({
          ...s,
          currentUser: userInfo,
        }));
      });
    }
  };

  const formatLoginErrorMessage = (msg?: string) => {
    if (!msg) {
      return undefined;
    }
    const text = msg.trim();
    const accountNotFoundPattern = /(账号|帐户)(不存在)|用户名|手机号|邮箱/;
    if (accountNotFoundPattern.test(text)) {
      return '工号不存在，请检查工号';
    }
    return text;
  };

  // 页面加载时，读取保存的账号密码并自动填充
  useEffect(() => {
    const savedAccount = getUserAccount();
    const savedPassword = getUserPassword();
    
    const values: { userName?: string; passWord?: string } = {};
    
    if (savedAccount) {
      values.userName = savedAccount;
    }
    
    if (savedPassword) {
      values.passWord = savedPassword;
      setRememberPassword(true);
    }
    
    // 设置初始值
    setInitialFormValues(values);
    
    // 延迟设置表单值，确保表单已经渲染完成
    const timer = setTimeout(() => {
      if (formRef.current && (values.userName || values.passWord)) {
        formRef.current.setFieldsValue(values);
      }
    }, 100);
    
    return () => clearTimeout(timer);
  }, []);

  const handleSubmit = async (values: UserLoginDto) => {
    try {
      // 登录 - 添加返回类型定义
      const response: LoginResponse = await login({ userName: values.userName,passWord: values.passWord }); 
      if (response.code === 200) {
        // 根据"记住密码"复选框状态保存账号和密码
        if (rememberPassword) {
          // 勾选了"记住密码"：保存账号和密码
          saveUserAccount(values.userName);
          saveUserPassword(values.passWord);
        } else {
          // 未勾选"记住密码"：只保存账号，清除密码
          saveUserAccount(values.userName);
          clearUserPassword();
        }

        // 存储token到本地存储
        localStorage.setItem('accessToken', response.token);
        localStorage.setItem('authToken', response.token);
        localStorage.setItem('permissions', JSON.stringify(response.permissions));
        localStorage.setItem('roles', JSON.stringify(response.roles));
        
        // 将 token 存入 Cookie（与 localStorage 保持一致）
        setTokenCookie(response.token);
        
        // 直接使用登录返回的用户信息，减少二次请求
        const userInfo = response.user;
        
        // 登录成功后，向 Cookie 写入认证标识值
        setAuthCookie(userInfo.userId);

        // 将用户名存入 Cookie
        setCookie(COOKIE_KEYS.LOGIN_USERNAME, values.userName);
        
        
        setInitialState({
          currentUser: {
            name: userInfo.nickName || userInfo.userName,
            avatar: userInfo.avatar,
            userid: userInfo.userId.toString(),
            email: userInfo.email,
            // 扩展属性，兼容类型定义中未声明的字段
            ...(userInfo.userName ? { userName: userInfo.userName } : {}),
            ...(userInfo.nickName ? { nickName: userInfo.nickName } : {}),
          } as any,
        });
      
        const defaultLoginSuccessMessage = intl.formatMessage({
          id: 'pages.login.success',
          defaultMessage: '登录成功！',
        });
        message.success(defaultLoginSuccessMessage);
      
        // 处理重定向
        const urlParams = new URL(window.location.href).searchParams;
        const redirectUrl = urlParams.get('redirect') || '/';
        window.location.href = redirectUrl;
        return;
      }
      
      const errorMsg = formatLoginErrorMessage(response.msg) || '登录失败，请检查工号或密码';
      message.error(errorMsg);
     //setUserLoginState(response);
    } catch (error: any) {
      // 网络错误或异常处理
      const formatted = formatLoginErrorMessage(error?.message);
      const fallback = intl.formatMessage({
        id:'pages.login.failure',
        defaultMessage:'登录失败，请重试！'
      });
      message.error(formatted || fallback);
     // console.error('登录异常:', error);
    }
  };

  const handleRegisterSuccess = ({ userName, password }: { userName: string; password: string }) => {
    setRegisterOpen(false);
    setRememberPassword(false);
    setInitialFormValues((prev) => ({
      ...prev,
      userName,
      passWord: password,
    }));
    if (formRef.current) {
      formRef.current.setFieldsValue({
        userName,
        passWord: password,
      });
    }
  };
  const { status, type: loginType } = userLoginState;

  return (
    <div className={styles.container}>
      <Helmet>
        <title>
          {intl.formatMessage({
            id: 'menu.login',
            defaultMessage: '登录页',
          })}
          {Settings.title && ` - ${Settings.title}`}
        </title>
      </Helmet>
      <Lang />
      <div
        style={{
          flex: '1',
          padding: '32px 0',
        }}
      >
        {/* 右侧居中容器，包裹 Logo 与登录表单 */}
        <div
          style={{
            width: 450,
            maxWidth: '90vw',
            margin: 'calc(22vh - 1.5cm) calc(10vw - 0.5cm) 0 auto',
            position: 'relative',
            minHeight: 382,
            padding: '32px 24px 28px',
            boxSizing: 'border-box',
          }}
        >
          {/* 背景面板：绝对定位，仅渲染一次 */}
          <div
            style={{
              position: 'absolute',
              inset: 0,
              backgroundImage: `url(${(window as any)?.publicPath || '/'}IOT-172.svg)`,
              backgroundRepeat: 'no-repeat',
              backgroundPosition: 'center calc(50% + 1.0cm)',
              backgroundSize: '85% 85%',
              pointerEvents: 'none',
            }}
          />
          {/* 自定义 Logo 区域：与容器同宽并置于其上方 */}
            <div
            style={{
              width: '100%',
              marginBottom: 16,
              position: 'relative',
              zIndex: 1,
              marginTop: '-1.5cm',
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'center',
              overflow: 'visible',
            }}
            >
            <img
              alt="logo"
              src={`${(window as any)?.publicPath || '/'}logo-19.svg`}
              style={{
              width: '100%',
              height: 100,
              display: 'block',
              objectFit: 'contain',
              transform: 'scale(1.15)',
              transformOrigin: 'center center',
               // 添加动态效果
               animation: 'slideInRight 2s ease-out',
              }}
            />
            </div>

          <LoginForm
            formRef={formRef}
            style={{
              width: 250,
              maxWidth: '90vw',
              margin: '0.2cm auto 0',
              position: 'relative',
              zIndex: 1,
            }}
          contentStyle={{
            minWidth: 200,
            maxWidth: '100%',
            width: '100%',
            textAlign: 'left',
          }}
          
          title={null}
          subTitle={null}
          initialValues={initialFormValues}
          // actions={[
          //   <FormattedMessage
          //     key="loginWith"
          //     id="pages.login.loginWith"
          //     defaultMessage="其他登录方式"
          //   />,
          //   <ActionIcons key="icons" />,
          // ]}
          onFinish={async (values) => {
            await handleSubmit(values as UserLoginDto);
          }}
        >
          {/** Tabs 被隐藏，仅保留账户密码登录 */}

          {status === 'error' && loginType === 'account' && (
            <LoginMessage
              content={intl.formatMessage({
                id: 'pages.login.accountLogin.errorMessage',
                defaultMessage: '账户或密码错误',
              })}
            />
          )}
          {/** 仅显示账户密码登录表单 */}
          <>
            <ProFormText
              name="userName"
              fieldProps={{
                size: 'large',
                prefix: <UserOutlined />,
              }}
              placeholder={intl.formatMessage({
              id: 'pages.login.username.placeholder',
              defaultMessage: '请输入工号',
              })}
              rules={[
                {
                  required: true,
                  message: '请输入工号',
                },
              ]}
            />
            <ProFormText.Password
              name="passWord"
              fieldProps={{
                size: 'large',
                prefix: <LockOutlined />,
              }}
              placeholder={intl.formatMessage({
                id: 'pages.login.password.placeholder',
                defaultMessage: '密码',
              })}
              rules={[
                {
                  required: true,
                  message: (
                    <FormattedMessage
                      id="pages.login.password.required"
                      defaultMessage="请输入密码！"
                    />
                  ),
                },
              ]}
            />
            <div
              style={{
                marginTop: 8,
                marginBottom: 12,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
              }}
            >
              <Checkbox
                checked={rememberPassword}
                onChange={(e) => setRememberPassword(e.target.checked)}
              >
                <span style={{ color: '#ffffff', fontSize: '14px' }}>记住密码</span>
              </Checkbox>
              {/* <Button
                type="link"
                style={{ padding: 0, color: '#40a9ff', fontSize: 14 }}
                onClick={() => setRegisterOpen(true)}
              >
                注册
              </Button> */}
            </div>
          </>

          {/** 手机号登录与验证码相关内容已注释，页面不再显示；自动登录与忘记密码已移除 */}
        </LoginForm>
        <RegisterModal
          open={registerOpen}
          onClose={() => setRegisterOpen(false)}
          onRegistered={handleRegisterSuccess}
        />
        </div>
      </div>
      <Footer />
    </div>
  );
};

export default Login;
