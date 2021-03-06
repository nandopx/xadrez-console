﻿using System;
using System.Collections.Generic;
using tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }

        public PartidaDeXadrez ()
        {
            this.tab = new Tabuleiro( 8, 8 );
            this.turno = 1;
            this.jogadorAtual = Cor.Branca;
            terminada = false;
            xeque = false;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            colocarPecas();
        }

        public Peca ExecutaMovimento ( Posicao origem, Posicao destino )
        {
            Peca p = tab.retirarPeca( origem );
            p.incrementarQteMovimentos();
            Peca pecaCapturada = tab.retirarPeca( destino );
            tab.colocarPeca( p, destino );
            if (pecaCapturada != null)
                capturadas.Add( pecaCapturada );
            return pecaCapturada;
        }

        public void desfazMovimento ( Posicao origem, Posicao destino, Peca pecaCapturada )
        {
            Peca p = tab.retirarPeca( destino );
            p.decrementarQteMovimentos();
            if (pecaCapturada != null)
            {
                tab.colocarPeca( pecaCapturada, destino );
                capturadas.Remove( pecaCapturada );
            }
            tab.colocarPeca( p, origem );
        }

        public void realizaJogada ( Posicao origem, Posicao destino )
        {
            Peca pecaCapturada = ExecutaMovimento( origem, destino );

            if (estaEmXeque( jogadorAtual ))
            {
                desfazMovimento( origem, destino, pecaCapturada );
                throw new TabuleiroException( "Você não pode se colocar em XEQUE!" );
            }

            if (estaEmXeque( adversaria( jogadorAtual ) ))
                xeque = true;
            else
                xeque = false;

            if (testeXequeMate( adversaria( jogadorAtual ) ))
                terminada = true;
            else
            {
                turno++;
                mudaJogador();
            }
        }

        public void validarPosicaoDeOrigem ( Posicao pos )
        {
            if (tab.peca( pos ) == null)
                throw new TabuleiroException( "Não existe peça na posição de origem escolhida!" );
            if (jogadorAtual != tab.peca( pos ).cor)
                throw new TabuleiroException( "A peça de origem escolhida não é sua!" );
            if (!tab.peca( pos ).existeMovimentosPossiveis())
                throw new TabuleiroException( "Não há movimentos possíveis para a peça de origem escolhida!" );
        }

        public void validarPosicaoDeDestino ( Posicao origem, Posicao destino )
        {
            if (!tab.peca( origem ).movimentoPossivel( destino ))
                throw new TabuleiroException( "Posição de destino inválida!" );
        }

        private void mudaJogador ()
        {
            if (jogadorAtual == Cor.Branca)
                jogadorAtual = Cor.Preta;
            else
                jogadorAtual = Cor.Branca;
        }

        public HashSet<Peca> pecasCapturadas ( Cor cor )
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca peca in capturadas)
            {
                if (peca.cor == cor)
                    aux.Add( peca );
            }
            return aux;
        }

        public HashSet<Peca> pecasEmJogo ( Cor cor )
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca peca in pecas)
            {
                if (peca.cor == cor)
                    aux.Add( peca );
            }
            aux.ExceptWith( pecasCapturadas( cor ) );
            return aux;
        }

        private Peca rei ( Cor cor )
        {
            foreach (Peca peca in pecasEmJogo( cor ))
            {
                if (peca is Rei)
                    return peca;
            }
            return null;
        }

        private Cor adversaria ( Cor cor )
        {
            if (cor == Cor.Branca)
                return Cor.Preta;
            else
                return Cor.Branca;
        }

        public bool estaEmXeque ( Cor cor )
        {
            Peca R = rei( cor );
            if (R == null)
                throw new TabuleiroException( "Não tem Rei da cor " + cor + " no tabuleiro!" );

            foreach (Peca peca in pecasEmJogo( adversaria( cor ) ))
            {
                bool[,] mat = peca.movimentosPossiveis();
                if (mat[R.posicao.linha, R.posicao.coluna])
                    return true;
            }
            return false;
        }

        public bool testeXequeMate ( Cor cor )
        {
            if (!estaEmXeque( cor ))
                return false;
            foreach (Peca peca in pecasEmJogo( cor ))
            {
                bool[,] mat = peca.movimentosPossiveis();
                for (int i = 0; i < tab.linhas; i++)
                {
                    for (int j = 0; j < tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = peca.posicao;
                            Posicao destino = new Posicao( i, j );
                            Peca pecaCapturada = ExecutaMovimento( origem, destino );
                            bool testeXeque = estaEmXeque( cor );
                            desfazMovimento( origem, destino, pecaCapturada );
                            if (!testeXeque)
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        public void colocarNovaPeca ( char coluna, int linha, Peca peca )
        {
            tab.colocarPeca( peca, new PosicaoXadrez( coluna, linha ).toPosicao() );
            pecas.Add( peca );
        }

        private void colocarPecas ()
        {
            colocarNovaPeca( 'c', 1, new Torre( tab, Cor.Branca ) );
            colocarNovaPeca( 'd', 1, new Rei( tab, Cor.Branca ) );
            colocarNovaPeca( 'h', 7, new Torre( tab, Cor.Branca ) );

            colocarNovaPeca( 'a', 8, new Rei( tab, Cor.Preta ) );
            colocarNovaPeca( 'b', 8, new Torre( tab, Cor.Preta ) );

            //colocarNovaPeca( 'c', 1, new Torre( tab, Cor.Branca ) );
            //colocarNovaPeca( 'c', 2, new Torre( tab, Cor.Branca ) );
            //colocarNovaPeca( 'd', 2, new Torre( tab, Cor.Branca ) );
            //colocarNovaPeca( 'e', 2, new Torre( tab, Cor.Branca ) );
            //colocarNovaPeca( 'e', 1, new Torre( tab, Cor.Branca ) );
            //colocarNovaPeca( 'd', 1, new Rei( tab, Cor.Branca ) );

            //colocarNovaPeca( 'c', 7, new Torre( tab, Cor.Preta ) );
            //colocarNovaPeca( 'c', 8, new Torre( tab, Cor.Preta ) );
            //colocarNovaPeca( 'd', 7, new Torre( tab, Cor.Preta ) );
            //colocarNovaPeca( 'e', 7, new Torre( tab, Cor.Preta ) );
            //colocarNovaPeca( 'e', 8, new Torre( tab, Cor.Preta ) );
            //colocarNovaPeca( 'd', 8, new Rei( tab, Cor.Preta ) );
        }
    }
}
