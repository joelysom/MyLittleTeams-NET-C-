'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { FirebaseError } from 'firebase/app';
import {
  createUserWithEmailAndPassword,
  sendPasswordResetEmail,
  signInWithEmailAndPassword,
  updateProfile,
} from 'firebase/auth';
import { auth } from '../../lib/firebase';
import { Home, Mail, Lock, ArrowRight, User, Eye, EyeOff, Phone, GraduationCap, Hash } from 'lucide-react';

const mapFirebaseErrorMessage = (code: string) => {
  switch (code) {
    case 'auth/wrong-password':
      return 'A senha informada está incorreta.';
    case 'auth/user-not-found':
      return 'Não encontramos uma conta com esse email.';
    case 'auth/invalid-email':
      return 'O email informado não é válido.';
    case 'auth/email-already-in-use':
      return 'Já existe uma conta registrada com esse email.';
    case 'auth/weak-password':
      return 'A senha deve ter pelo menos 6 caracteres.';
    case 'auth/too-many-requests':
      return 'Muitas tentativas. Tente novamente mais tarde.';
    case 'auth/network-request-failed':
      return 'Falha de conexão. Verifique sua internet e tente novamente.';
    default:
      return 'Ocorreu um erro durante a autenticação. Tente novamente.';
  }
};

export default function LoginPage() {
  const router = useRouter();
  const [isLogin, setIsLogin] = useState(true);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [phone, setPhone] = useState('');
  const [role, setRole] = useState('student');
  const [course, setCourse] = useState('');
  const [registration, setRegistration] = useState('');
  const [department, setDepartment] = useState('');
  const [focus, setFocus] = useState('');
  const [officeHours, setOfficeHours] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [keepLoggedIn, setKeepLoggedIn] = useState(true);
  const [agreeTerms, setAgreeTerms] = useState(false);
  const [loading, setLoading] = useState(false);
  const [feedback, setFeedback] = useState<{ type: 'error' | 'success'; message: string } | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    setFeedback(null);
    if (!email.trim() || !password) {
      setFeedback({ type: 'error', message: 'Preencha todos os campos necessários.' });
      return;
    }

    if (!isLogin) {
      if (!username.trim() || !phone.trim() || !course.trim() || !registration.trim()) {
        setFeedback({ type: 'error', message: 'Preencha todos os campos necessários.' });
        return;
      }
      if (role === 'professor' && (!department.trim() || !focus.trim() || !officeHours.trim())) {
        setFeedback({ type: 'error', message: 'Preencha todos os campos necessários para professores.' });
        return;
      }
      if (password !== confirmPassword) {
        setFeedback({ type: 'error', message: 'As senhas não coincidem.' });
        return;
      }
      if (!agreeTerms) {
        setFeedback({ type: 'error', message: 'Você deve concordar com os Termos de Serviço.' });
        return;
      }
    }

    setLoading(true);

    try {
      if (isLogin) {
        await signInWithEmailAndPassword(auth, email.trim(), password);
      } else {
        const userCredential = await createUserWithEmailAndPassword(auth, email.trim(), password);
        if (userCredential.user && username.trim()) {
          await updateProfile(userCredential.user, { displayName: username.trim() });
        }
        // Here you would typically save additional user data to Firestore
        // For now, just create the account
      }
      router.push('/');
    } catch (error) {
      if (error instanceof FirebaseError) {
        setFeedback({ type: 'error', message: mapFirebaseErrorMessage(error.code) });
      } else {
        setFeedback({ type: 'error', message: 'Falha ao processar sua solicitação. Tente novamente.' });
      }
    } finally {
      setLoading(false);
    }
  };

  const handleForgotPassword = async () => {
    if (!email.trim()) {
      setFeedback({ type: 'error', message: 'Digite seu email para recuperar a senha.' });
      return;
    }

    setFeedback(null);
    setLoading(true);

    try {
      await sendPasswordResetEmail(auth, email.trim());
      setFeedback({ type: 'success', message: 'Email de recuperação enviado. Verifique sua caixa de entrada.' });
    } catch (error) {
      if (error instanceof FirebaseError) {
        setFeedback({ type: 'error', message: mapFirebaseErrorMessage(error.code) });
      } else {
        setFeedback({ type: 'error', message: 'Não foi possível enviar o email de recuperação. Tente novamente.' });
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      className="relative min-h-screen w-full flex items-center justify-center p-5"
      style={{
        backgroundImage: 'url(/img/Hero_0.png)',
        backgroundPosition: 'center',
        backgroundSize: 'cover',
        backgroundAttachment: 'fixed',
        backgroundRepeat: 'no-repeat',
      }}
    >
      {/* Overlay */}
      <div className="absolute inset-0 bg-black/50 backdrop-blur-md" />

      {/* Form Container */}
      <div className="relative w-full max-w-4xl h-auto md:h-[700px] flex overflow-hidden rounded-3xl border-2 border-white/30 backdrop-blur-2xl bg-white/20 shadow-2xl">
        
        {/* Left Column - Image Layer */}
        <div className="hidden md:flex w-1/2 flex-col items-center justify-start bg-gradient-to-br from-white/30 to-transparent backdrop-blur-3xl rounded-l-3xl p-8 relative overflow-hidden pt-20">
          {/* Animated Background Elements */}
          <div className="absolute top-20 inset-x-0 flex items-center justify-center">
            {/* Main Icon - Choas */}
            <div className="relative w-80 h-80 flex items-center justify-center">
              {/* Animated Circle */}
              <div className="absolute inset-0 rounded-full border-2 border-white/30 animate-pulse shadow-lg shadow-cyan-500/20" />
              <div className="absolute inset-12 rounded-full border border-white/20 animate-spin" style={{ animationDuration: '6s' }} />
              
              {/* Icon */}
              <div className="relative z-20 text-white">
                <img src="/img/LoginICO.png" alt="Choas" className="w-48 h-48 animate-bounce drop-shadow-2xl" />
              </div>

              {/* Floating Elements */}
              <div className="absolute top-16 left-16 w-16 h-16 rounded-full bg-cyan-400 opacity-50 animate-pulse shadow-lg" style={{ animationDelay: '0.2s' }} />
              <div className="absolute bottom-20 right-20 w-20 h-20 rounded-full bg-indigo-400 opacity-40 animate-pulse shadow-lg" style={{ animationDelay: '0.5s' }} />
              <div className="absolute top-1/2 right-12 w-14 h-14 rounded-full bg-cyan-300 opacity-35 animate-pulse" style={{ animation: 'float 4s ease-in-out infinite' }} />
            </div>
          </div>

          {/* Text */}
          <div className="relative z-0 text-center mt-auto mb-8 flex-shrink-0">
            <div className="inline-block px-6 py-4 rounded-2xl bg-black/40 backdrop-blur-md border border-white/20 shadow-2xl">
              <p className="text-white text-xl font-bold text-center max-w-xs leading-relaxed drop-shadow-lg">
                Você está a poucos minutos de potencializar sua experiência com{' '}
                <span className="text-cyan-300 font-extrabold text-2xl drop-shadow-2xl">Choas</span>
              </p>
            </div>
          </div>
        </div>

        {/* Right Column - Forms */}
        <div className="w-full md:w-1/2 flex flex-col items-center justify-start pt-6 px-6 md:px-8 relative z-10">
          {/* Home Button */}
          <Link
            href="/"
            className="absolute top-4 left-4 p-2 rounded-full hover:bg-white/20 transition text-white"
            title="Voltar para home"
          >
            <Home size={24} />
          </Link>

          {/* Buttons Toggles */}
          <div className="flex gap-3 mb-8">
            <button
              onClick={() => setIsLogin(true)}
              className={`px-8 py-2 rounded-full font-semibold transition ${
                isLogin
                  ? 'bg-slate-900 text-white'
                  : 'bg-white/20 text-white hover:bg-white/30'
              }`}
            >
              Entrar
            </button>
            <button
              onClick={() => setIsLogin(false)}
              className={`px-8 py-2 rounded-full font-semibold transition ${
                !isLogin
                  ? 'bg-slate-900 text-white'
                  : 'bg-white/20 text-white hover:bg-white/30'
              }`}
            >
              Cadastrar
            </button>
          </div>

          {/* Form Container */}
          <div className="w-full max-w-sm max-h-[520px] overflow-y-auto px-4">
            {/* Title */}
            <h1 className="text-3xl font-bold text-white mb-6 text-center">
              {isLogin ? 'Entrar' : 'Criar Conta'}
            </h1>

            <form onSubmit={handleSubmit} className="space-y-4">
              {isLogin ? (
                // Login Form
                <>
                  <div className="relative">
                    <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type="text"
                      placeholder="Email ou Telefone"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      className="w-full h-14 pl-12 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                  </div>

                  <div className="relative">
                    <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type={showPassword ? 'text' : 'password'}
                      placeholder="Senha"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      className="w-full h-14 pl-12 pr-12 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-white opacity-70 hover:opacity-100 transition"
                    >
                      {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                    </button>
                  </div>

                  <div className="flex items-center text-white">
                    <input
                      type="checkbox"
                      id="keepLoggedIn"
                      checked={keepLoggedIn}
                      onChange={(e) => setKeepLoggedIn(e.target.checked)}
                      className="mr-2"
                    />
                    <label htmlFor="keepLoggedIn" className="text-sm">
                      Manter conectado
                    </label>
                  </div>

                  <button
                    type="submit"
                    className="w-full h-14 bg-slate-900 hover:bg-slate-800 text-white font-semibold rounded-xl flex items-center justify-center gap-2 transition mt-6 shadow-lg hover:shadow-xl hover:gap-4"
                    disabled={loading}
                  >
                    <span>{loading ? 'Entrando...' : 'Entrar'}</span>
                    <ArrowRight size={20} />
                  </button>

                  <div className="text-center">
                    <button
                      type="button"
                      onClick={handleForgotPassword}
                      className="text-white text-sm hover:underline hover:text-cyan-300 transition"
                    >
                      Esqueceu a senha?
                    </button>
                  </div>
                </>
              ) : (
                // Signup Form
                <>
                  <div className="relative">
                    <User className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type="text"
                      placeholder="Nome Completo"
                      value={username}
                      onChange={(e) => setUsername(e.target.value)}
                      className="w-full h-14 pl-12 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                  </div>

                  <div className="relative">
                    <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type="email"
                      placeholder="Email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      className="w-full h-14 pl-12 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                  </div>

                  <div className="relative">
                    <Phone className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type="tel"
                      placeholder="Telefone"
                      value={phone}
                      onChange={(e) => setPhone(e.target.value)}
                      className="w-full h-14 pl-12 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                  </div>

                  <div className="relative">
                    <select
                      value={role}
                      onChange={(e) => setRole(e.target.value)}
                      className="w-full h-14 pl-12 pr-12 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur appearance-none"
                    >
                      <option value="student" className="text-black">Aluno</option>
                      <option value="professor" className="text-black">Professor orientador</option>
                    </select>
                    <User className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <div className="absolute right-4 top-1/2 -translate-y-1/2 text-white opacity-70">▼</div>
                  </div>

                  <div className="relative">
                    <GraduationCap className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type="text"
                      placeholder="Curso"
                      value={course}
                      onChange={(e) => setCourse(e.target.value)}
                      className="w-full h-14 pl-12 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                  </div>

                  <div className="relative">
                    <Hash className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type="text"
                      placeholder="Matrícula"
                      value={registration}
                      onChange={(e) => setRegistration(e.target.value)}
                      className="w-full h-14 pl-12 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                  </div>

                  {role === 'professor' && (
                    <>
                      <div className="p-4 bg-white/20 border border-white/30 rounded-xl text-white text-sm">
                        O cadastro docente libera dashboard multi-equipes, exportação acadêmica e permissões avançadas de orientação.
                      </div>

                      <input
                        type="text"
                        placeholder="Departamento ou área"
                        value={department}
                        onChange={(e) => setDepartment(e.target.value)}
                        className="w-full h-14 pl-4 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                        required
                      />

                      <input
                        type="text"
                        placeholder="Foco acadêmico"
                        value={focus}
                        onChange={(e) => setFocus(e.target.value)}
                        className="w-full h-14 pl-4 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                        required
                      />

                      <input
                        type="text"
                        placeholder="Janela de atendimento"
                        value={officeHours}
                        onChange={(e) => setOfficeHours(e.target.value)}
                        className="w-full h-14 pl-4 pr-4 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                        required
                      />
                    </>
                  )}

                  <div className="relative">
                    <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type={showPassword ? 'text' : 'password'}
                      placeholder="Senha"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      className="w-full h-14 pl-12 pr-12 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-white opacity-70 hover:opacity-100 transition"
                    >
                      {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                    </button>
                  </div>

                  <div className="relative">
                    <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-white opacity-70" size={20} />
                    <input
                      type={showConfirmPassword ? 'text' : 'password'}
                      placeholder="Confirmar Senha"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      className="w-full h-14 pl-12 pr-12 bg-white/20 border border-white/30 rounded-xl text-white placeholder-white/70 focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:bg-white/30 transition backdrop-blur"
                      required
                    />
                    <button
                      type="button"
                      onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-white opacity-70 hover:opacity-100 transition"
                    >
                      {showConfirmPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                    </button>
                  </div>

                  <div className="flex items-center text-white">
                    <input
                      type="checkbox"
                      id="agreeTerms"
                      checked={agreeTerms}
                      onChange={(e) => setAgreeTerms(e.target.checked)}
                      className="mr-2"
                    />
                    <label htmlFor="agreeTerms" className="text-sm">
                      Concordo com os Termos de Serviço
                    </label>
                  </div>

                  <button
                    type="submit"
                    className="w-full h-14 bg-slate-900 hover:bg-slate-800 text-white font-semibold rounded-xl flex items-center justify-center gap-2 transition mt-4 shadow-lg hover:shadow-xl hover:gap-4"
                    disabled={loading}
                  >
                    <span>{loading ? 'Criando...' : 'Cadastrar'}</span>
                    <ArrowRight size={20} />
                  </button>
                </>
              )}
            </form>

            {/* Feedback */}
            {feedback && (
              <div className={`mt-4 p-3 rounded-xl ${
                feedback.type === 'error' ? 'bg-red-500/20 border border-red-500/30 text-red-200' : 'bg-green-500/20 border border-green-500/30 text-green-200'
              }`}>
                {feedback.message}
              </div>
            )}

            {/* Divider */}
            <div className="flex items-center gap-4 my-6">
              <div className="flex-1 h-px bg-white/20" />
              <span className="text-white/60 text-sm">ou</span>
              <div className="flex-1 h-px bg-white/20" />
            </div>

            {/* Social Login */}
            <div className="flex gap-4 justify-center">
              <button
                type="button"
                className="w-12 h-12 rounded-full bg-white/20 hover:bg-white/30 flex items-center justify-center text-white transition"
                title="Google"
              >
                <span className="text-lg font-bold">G</span>
              </button>
              <button
                type="button"
                className="w-12 h-12 rounded-full bg-white/20 hover:bg-white/30 flex items-center justify-center text-white transition"
                title="GitHub"
              >
                <span className="text-lg font-bold">GH</span>
              </button>
            </div>

            {/* Sign Up / Sign In Link */}
            <p className="text-center text-white/70 text-sm mt-6">
              {isLogin ? "Não tem uma conta? " : 'Já tem uma conta? '}
              <button
                onClick={() => setIsLogin(!isLogin)}
                className="text-cyan-300 hover:text-cyan-200 font-semibold transition"
              >
                {isLogin ? 'Cadastre-se' : 'Entre'}
              </button>
            </p>
          </div>
        </div>
      </div>

      <style>{`
        @keyframes float {
          0%, 100% {
            transform: translateY(0px);
          }
          50% {
            transform: translateY(-20px);
          }
        }

        @keyframes bounce {
          0%, 100% {
            transform: translateY(0);
          }
          50% {
            transform: translateY(-10px);
          }
        }
      `}</style>
    </div>
  );
}
