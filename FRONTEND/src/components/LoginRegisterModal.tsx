import React, { useState } from 'react';
import { api } from '../api/api';
import type { AuthResponseDTO } from '../types/viagem';
import { X, Loader2, LogIn, UserPlus } from 'lucide-react';

interface LoginRegisterModalProps {
  aberto: boolean;
  onFechar: () => void;
  onSucesso: (auth: AuthResponseDTO) => void;
  motivo?: string;
}

export const LoginRegisterModal: React.FC<LoginRegisterModalProps> = ({
  aberto,
  onFechar,
  onSucesso,
  motivo,
}) => {
  const [modo, setModo] = useState<'login' | 'registar'>('login');
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  if (!aberto) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro(null);
    setCarregando(true);

    try {
      const resposta = modo === 'login'
        ? await api.post<AuthResponseDTO>('/auth/login', { email, password })
        : await api.post<AuthResponseDTO>('/auth/registar', { nome, email, password });

      onSucesso(resposta.data);
    } catch (err: any) {
      setErro(err.response?.data?.erro || 'Não foi possível autenticar. Verifica os dados.');
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-[#17324B]/60 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl w-full max-w-sm overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-[#E4E1D8]">
          <h2
            className="text-xl text-[#17324B]"
            style={{ fontFamily: "'Fraunces', serif", fontStyle: 'italic', fontWeight: 500 }}
          >
            {modo === 'login' ? 'Entrar' : 'Criar conta'}
          </h2>
          <button onClick={onFechar} className="text-[#9AA3AB] hover:text-[#17324B]">
            <X size={20} />
          </button>
        </div>

        {motivo && (
          <div className="px-6 pt-4 text-xs text-[#4B5A68]">{motivo}</div>
        )}

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {modo === 'registar' && (
            <div>
              <label className="block text-xs text-[#4B5A68] mb-1">Nome</label>
              <input
                type="text"
                required
                value={nome}
                onChange={(e) => setNome(e.target.value)}
                className="w-full border-b border-[#D9D5C9] focus:border-[#17324B] focus:outline-none py-2 text-sm"
              />
            </div>
          )}

          <div>
            <label className="block text-xs text-[#4B5A68] mb-1">Email</label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full border-b border-[#D9D5C9] focus:border-[#17324B] focus:outline-none py-2 text-sm"
            />
          </div>

          <div>
            <label className="block text-xs text-[#4B5A68] mb-1">Password</label>
            <input
              type="password"
              required
              minLength={6}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full border-b border-[#D9D5C9] focus:border-[#17324B] focus:outline-none py-2 text-sm"
            />
          </div>

          {erro && <p className="text-xs text-[#c1440e]">{erro}</p>}

          <button
            type="submit"
            disabled={carregando}
            className="w-full inline-flex items-center justify-center gap-2 px-4 py-2.5 bg-[#17324B] text-white font-semibold rounded-full hover:bg-[#20476b] transition disabled:opacity-50"
          >
            {carregando ? (
              <Loader2 size={16} className="animate-spin" />
            ) : modo === 'login' ? (
              <LogIn size={16} />
            ) : (
              <UserPlus size={16} />
            )}
            <span>{modo === 'login' ? 'Entrar' : 'Criar conta'}</span>
          </button>

          <p className="text-xs text-center text-[#4B5A68]">
            {modo === 'login' ? 'Ainda não tens conta?' : 'Já tens conta?'}{' '}
            <button
              type="button"
              onClick={() => {
                setErro(null);
                setModo(modo === 'login' ? 'registar' : 'login');
              }}
              className="text-[#0E6B63] font-semibold hover:underline"
            >
              {modo === 'login' ? 'Criar conta' : 'Entrar'}
            </button>
          </p>
        </form>
      </div>
    </div>
  );
};
