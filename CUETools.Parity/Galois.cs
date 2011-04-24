using System;
using System.Collections.Generic;
using System.Text;

namespace CUETools.Parity
{
	public class Galois
	{
		private ushort[] expTbl; // 二重にもつことによりmul, div等を簡略化
		private ushort[] logTbl;
		private int w;
		private int max;
		private int symStart = 0;

		/**
		 * スカラー、ベクターの相互変換テーブルの作成
		 */
		public Galois(int polynomial, int _w)
		{
			w = _w;
			max = (1 << _w) - 1;

			expTbl = new ushort[max * 2];
			logTbl = new ushort[max + 1];

			int d = 1;
			for (int i = 0; i < max; i++)
			{
				//if (d == 0)
				//    throw new Exception("oops");
				expTbl[i] = expTbl[max + i] = (ushort)d;
				logTbl[d] = (ushort)i;
				d <<= 1;
				if (((d >> _w) & 1) != 0)
					d = (d ^ polynomial) & max;
			}
		}

		public int Max
		{
			get
			{
				return max;
			}
		}

		public ushort[] ExpTbl
		{
			get
			{
				return expTbl;
			}
		}

		public ushort[] LogTbl
		{
			get
			{
				return logTbl;
			}
		}

		/**
		 * スカラー -> ベクター変換
		 *
		 * @param a int
		 * @return int
		 */
		public int toExp(int a)
		{
			return expTbl[a];
		}

		/**
		 * ベクター -> スカラー変換
		 *
		 * @param a int
		 * @return int
		 */
		public int toLog(int a)
		{
			return logTbl[a];
		}

		/**
		 * 誤り位置インデックスの計算
		 *
		 * @param length int
		 * 		データ長
		 * @param a int
		 * 		誤り位置ベクター
		 * @return int
		 * 		誤り位置インデックス
		 */
		public int toPos(int length, int a)
		{
			return length - 1 - logTbl[a];
		}

		/**
		 * 掛け算
		 *
		 * @param a int
		 * @param b int
		 * @return int
		 * 		= a * b
		 */
		public int mul(int a, int b)
		{
			return (a == 0 || b == 0) ? 0 : expTbl[(int)logTbl[a] + logTbl[b]];
		}

		/**
		 * 掛け算
		 *
		 * @param a int
		 * @param b int
		 * @return int
		 * 		= a * α^b
		 */
		public int mulExp(int a, int b)
		{
			return (a == 0) ? 0 : expTbl[logTbl[a] + b];
		}

		/**
		 * 割り算
		 *
		 * @param a int
		 * @param b int
		 * @return int
		 * 		= a / b
		 */
		public int div(int a, int b)
		{
			return (a == 0) ? 0 : expTbl[logTbl[a] - logTbl[b] + max];
		}

		/**
		 * 割り算
		 *
		 * @param a int
		 * @param b int
		 * @return int
		 * 		= a / α^b
		 */
		public int divExp(int a, int b)
		{
			return (a == 0) ? 0 : expTbl[logTbl[a] - b + max];
		}

		/**
		 * 逆数
		 *
		 * @param a int
		 * @return int
		 * 		= 1/a
		 */
		public int inv(int a)
		{
			return expTbl[max - logTbl[a]];
		}

		public int[] toLog(int[] a)
		{
			var res = new int[a.Length];
			for (int i = 0; i < a.Length; i++)
				res[i] = a[i] == 0 ? - 1 : toLog(a[i]);
			return res;
		}

		public int[] toLog(ushort[] a)
		{
			var res = new int[a.Length];
			for (int i = 0; i < a.Length; i++)
				res[i] = a[i] == 0 ? -1 : toLog(a[i]);
			return res;
		}

		public int[] toExp(int[] a)
		{
			var res = new int[a.Length];
			for (int i = 0; i < a.Length; i++)
				res[i] = a[i] == -1 ? 0 : toExp(a[i]);
			return res;
		}

		public int gfadd(int a, int b)
		{
			var a_exp = a == -1 ? 0 : toExp(a);
			var b_exp = b == -1 ? 0 : toExp(b);
			var res_exp = a_exp ^ b_exp;
			return res_exp == 0 ? -1 : toLog(res_exp);
		}

		public int[] gfadd(int[] a, int b)
		{
			var res = new int[a.Length];
			var a_exp = toExp(a);
			var b_exp = b == -1 ? 0 : toExp(b);			
			for (int i = 0; i < a.Length; i++)
				res[i] = a_exp[i] ^ b_exp;
			return toLog(res);
		}

		public int[] gfdiff(int[] a)
		{
			//l = length(polynomial);
			//for cc = 2:l
			//        %cc-1 represents the power of x
			//        if mod(cc-1,2) == 0 %all the even powers are zero because of GF(2)
			//            diff(cc-1) = -Inf; 
			//        else
			//            diff(cc-1) = polynomial(cc);
			//        end
			//end		
			var res = new int[a.Length - 1];
			for (int i = 0; i < res.Length; i++)
				res[i] = (i % 2) == 0 ? a[i + 1] : -1;
			return res;
		}

		public int gfmul(int a, int b)
		{
			return a < 0 || b < 0 ? -1 : ((a + b) % max);
		}

		public int gfdiv(int a, int b)
		{
			return a < 0 ? -1 : ((max + a - b) % max);
		}

		public int gfpow(int value, int p)
		{
			return (value * p) % max;
		}

		public int gfsubstitute(int[] polynomial, int value, int terms)
		{
			var sum = 0;
			if (value != -1)
				for (int p = 0; p < terms; p++)
					if (polynomial[p] != -1)
					{
						var pow = polynomial[p] + value * p;
						sum ^= expTbl[(pow & max) + (pow >> w)];
					}
			return sum == 0 ? -1 : logTbl[sum];
		}

		public int[] gfconv(int[] a, int[] b)
		{
			return gfconv(a, b, a.Length + b.Length - 1);
		}

		public int[] gfconv(int[] a, int[] b, int len)
		{
			var seki = new int[len];
			for (int ia = 0; ia < a.Length; ia++)
			{
				var loga = a[ia];
				if (loga != -1)
				{
					int ib2 = Math.Min(b.Length, len - ia);
					for (int ib = 0; ib < ib2; ib++)
					{
						var logb = b[ib];
						if (logb != -1)
							seki[ia + ib] ^= expTbl[loga + logb]; // = a[ia] * b[ib]
					}
				}
			}
			for (int i = 0; i < len; i++)
				seki[i] = seki[i] == 0 ? -1 : logTbl[seki[i]];
			return seki;
		}

		public unsafe void gfconv(int* a, int alen, int* b, int blen, int* c, int clen)
		{
			for (int i = 0; i < clen; i++)
				c[i] = 0;
			for (int ia = 0; ia < alen; ia++)
			{
				var loga = a[ia];
				if (loga != -1)
				{
					int ib2 = Math.Min(blen, clen - ia);
					for (int ib = 0; ib < ib2; ib++)
					{
						var logb = b[ib];
						if (logb != -1)
							c[ia + ib] ^= expTbl[loga + logb]; // = a[ia] * b[ib]
					}
				}
			}
			for (int i = 0; i < clen; i++)
				c[i] = c[i] == 0 ? -1 : logTbl[c[i]];
		}

		public int[] mulPoly(int[] a, int[] b)
		{
			return mulPoly(a, b, a.Length + b.Length - 1);
		}

		public int[] mulPoly(int[] a, int[] b, int len)
		{
			var res = new int[len];
			mulPoly(res, a, b);
			return res;
		}

		private static int[] erasures_pos;
		private static int[] erasure_loc_pol;
		private static int[] erasure_diff;

		public unsafe void Syndrome2Parity(ushort* syndrome, ushort* parity, int npar)
		{
			if (erasure_loc_pol == null)
			{
				var num_erasures = npar;

				// Compute the erasure locator polynomial:
				erasures_pos = new int[num_erasures];
				for (int x = 0; x < num_erasures; x++)
					erasures_pos[x] = x;

				//%Compute the erasure-locator polynomial
				// Optimized version
				var erasure_loc_pol_exp = new int[num_erasures + 1];
				erasure_loc_pol_exp[0] = 1;
				for (int i = 0; i < num_erasures; i++)
					for (int x = num_erasures; x > 0; x--)
						erasure_loc_pol_exp[x] ^= mulExp(erasure_loc_pol_exp[x - 1], erasures_pos[i]);
				erasure_loc_pol = toLog(erasure_loc_pol_exp);
				erasure_diff = gfdiff(erasure_loc_pol);
			}

			// Advance syndrome by npar zeroes (for npar 'erased' parity symbols)
			var S_pol = new int[npar + 1];
			S_pol[0] = -1;
			for (int i = 0; i < npar; i++)
			{
				if (syndrome[i] == 0)
					S_pol[i + 1] = -1;
				else
				{
					var exp = logTbl[syndrome[i]] + npar * i;
					S_pol[i + 1] = (exp & max) + (exp >> w);
				}
			}
			var mod_syn = gfconv(erasure_loc_pol, S_pol, npar + 1);

			//%Calculate remaining errors (non-erasures)
			//
			//S_M = [];
			//for i = 1:h - num_erasures
			//    S_M(i) = mod_syn(i + num_erasures + 1);
			//end		    
			//flag = 0;
			//if isempty(S_M) == 1
			//    flag = 0;
			//else
			//    for i = 1:length(S_M)
			//        if (S_M(i) ~= -Inf)
			//            flag = 1;     %Other errors occured in conjunction with erasures
			//        end
			//    end
			//end
			//%Find error-location polynomial sigma (Berlekamp's iterative algorithm - 
			//if (flag == 1)
			//{
			// ...
			//}
			//else
			//{
			//    sigma = 0;
			//    comp_error_locs = [];
			//}

#if kjsljdf
			var sigma = new int[1] { 0 };
			var omega = gfconv(sigma, gfadd(mod_syn, 0), npar + 1);
			var tsi = gfconv(sigma, erasure_loc_pol);
			var tsi_diff = gfdiff(tsi);
			//var e_e_places = [erasures_pos comp_error_locs];
#else
			var omega = mod_syn;
			var tsi_diff = erasure_diff;
			var e_e_places = erasures_pos;
#endif

			//%Calculate the error magnitudes
			//%Substitute the inverse into sigma_diff
			//var ERR = new int[e_e_places.Length];
			for (int ii = 0; ii < e_e_places.Length; ii++)
			{
				var point = max - e_e_places[ii];
				var ERR_DEN = gfsubstitute(tsi_diff, point, tsi_diff.Length);
				var ERR_NUM = gfsubstitute(omega, point, omega.Length);
				// Additional +ii because we use slightly different syndrome
				var pow = ERR_NUM + e_e_places[ii] + ii + max - ERR_DEN;
				
				//ERR[ii] = ERR_NUM == -1 ? 0 : expTbl[(pow & max) + (pow >> w)];
				parity[npar - 1 - ii] = ERR_NUM == -1 ? (ushort)0 : expTbl[(pow & max) + (pow >> w)];
			}
		}

		/**
		 * 数式の掛け算
		 *
		 * @param seki int[]
		 * 		seki = a * b
		 * @param a int[]
		 * @param b int[]
		 */
		public void mulPoly(int[] seki, int[] a, int[] b)
		{
			Array.Clear(seki, 0, seki.Length);
			for (int ia = 0; ia < a.Length; ia++)
			{
				if (a[ia] != 0)
				{
					int loga = logTbl[a[ia]];
					int ib2 = Math.Min(b.Length, seki.Length - ia);
					for (int ib = 0; ib < ib2; ib++)
					{
						if (b[ib] != 0)
						{
							seki[ia + ib] ^= expTbl[loga + logTbl[b[ib]]];	// = a[ia] * b[ib]
						}
					}
				}
			}
		}

		public unsafe void mulPoly(int* seki, int* a, int* b, int lenS, int lenA, int lenB)
		{
			for (int i = 0; i < lenS; i++)
				seki[i] = 0;
			for (int ia = 0; ia < lenA; ia++)
			{
				if (a[ia] != 0)
				{
					int loga = logTbl[a[ia]];
					int ib2 = Math.Min(lenB, lenS - ia);
					for (int ib = 0; ib < ib2; ib++)
					{
						if (b[ib] != 0)
						{
							seki[ia + ib] ^= expTbl[loga + logTbl[b[ib]]];	// = a[ia] * b[ib]
						}
					}
				}
			}
		}

		/**
		 * 生成多項式配列の作成
		 *		G(x)=Π[k=0,n-1](x + α^k)
		 *		encodeGxの添え字と次数の並びが逆なのに注意
		 *		encodeGx[0]        = x^(npar - 1)の項
		 *		encodeGx[1]        = x^(npar - 2)の項
		 *		...
		 *		encodeGx[npar - 1] = x^0の項
		 */
		public int[] makeEncodeGx(int npar)
		{
			int[] encodeGx = new int[npar];
			encodeGx[npar - 1] = 1;
			for (int i = 0, kou = symStart; i < npar; i++, kou++)
			{
				int ex = toExp(kou); // ex = α^kou
				// (x + α^kou)を掛る
				for (int j = 0; j < npar - 1; j++)
				{
					// 現在の項 * α^kou + 一つ下の次数の項
					encodeGx[j] = mul(encodeGx[j], ex) ^ encodeGx[j + 1];
				}
				encodeGx[npar - 1] = mul(encodeGx[npar - 1], ex);// 最下位項の計算
			}
			return encodeGx;
		}

		public int[] makeEncodeGxLog(int npar)
		{
			int[] encodeGx = makeEncodeGx(npar);
			for (int i = 0; i < npar; i++)
			{
				if (encodeGx[i] == 0)
					throw new Exception("0 in encodeGx");
				encodeGx[i] = toLog(encodeGx[i]);
			}
			return encodeGx;
		}

		/// <summary>
		/// parityTable[xx, 0, i] = mul(00xx, encodeGx[i])
		/// parityTable[xx, 1, i] = mul(xx00, encodeGx[i])
		/// </summary>
		/// <param name="npar"></param>
		/// <returns></returns>
		public ushort[,,] makeEncodeTable(int npar)
		{
			var loggx = makeEncodeGxLog(npar);
			var parityTable = new ushort[256, 2, npar];
			for (int i = 0; i < npar; i++)
			{
				parityTable[0, 0, i] = 0;
				parityTable[0, 1, i] = 0;
			}
			for (int ib = 1; ib < 256; ib++)
			{
				int logib0 = LogTbl[ib];
				int logib1 = LogTbl[ib << 8];
				for (int i = 0; i < npar; i++)
				{
					parityTable[ib, 0, i] = ExpTbl[logib0 + loggx[i]];
					parityTable[ib, 1, i] = ExpTbl[logib1 + loggx[i]];
				}
			}
			return parityTable;
		}

		/// <summary>
		/// parityTable[xx, 0, i] = mul(00xx, α^i)
		/// parityTable[xx, 1, i] = mul(xx00, α^i)
		/// </summary>
		/// <param name="npar"></param>
		/// <returns></returns>
		public ushort[,,] makeDecodeTable(int npar)
		{
			var parityTable = new ushort[256, 2, npar];
			for (int i = 0; i < npar; i++)
			{
				parityTable[0, 0, i] = 0;
				parityTable[0, 1, i] = 0;
			}
			for (int ib = 1; ib < 256; ib++)
			{
				int logib0 = LogTbl[ib];
				int logib1 = LogTbl[ib << 8];
				for (int i = 0; i < npar; i++)
				{
					parityTable[ib, 0, i] = ExpTbl[logib0 + i];
					parityTable[ib, 1, i] = ExpTbl[logib1 + i];
				}
			}
			return parityTable;
		}

		/**
		 * シンドロームの計算
		 * @param data int[]
		 *		入力データ配列
		 * @param length int
		 *		データ長
		 * @param syn int[]
		 *		(x - α^0) (x - α^1) (x - α^2) ...のシンドローム
		 * @return boolean
		 *		true: シンドロームは総て0
		 */
		public bool calcSyndrome(byte[] data, int length, int[] syn)
		{
			int hasErr = 0;
			for (int i = 0; i < syn.Length; i++)
			{
				int wk = 0;
				for (int idx = 0; idx < length; idx++)
				{
					//wk = data[idx] ^ ((wk == 0) ? 0 : expTbl[logTbl[wk] + i + symStart]);		// wk = data + wk * α^i
					wk = data[idx] ^ ((wk == 0) ? 0 : expTbl[logTbl[wk] + i]);		// wk = data + wk * α^i
				}
				syn[i] = wk;
				hasErr |= wk;
			}
			return hasErr == 0;
		}

		/**
		 * シンドロームの計算
		 * @param data int[]
		 *		入力データ配列
		 * @param length int
		 *		データ長
		 * @param syn int[]
		 *		(x - α^0) (x - α^1) (x - α^2) ...のシンドローム
		 * @return boolean
		 *		true: シンドロームは総て0
		 */
		public unsafe bool calcSyndrome(ushort* data, int length, int[] syn)
		{
			int hasErr = 0;
			for (int i = 0; i < syn.Length; i++)
			{
				int wk = 0;
				for (int idx = 0; idx < length; idx++)
				{
					//wk = data[idx] ^ ((wk == 0) ? 0 : expTbl[logTbl[wk] + i + symStart]);		// wk = data + wk * α^i
					wk = data[idx] ^ ((wk == 0) ? 0 : expTbl[logTbl[wk] + i]);		// wk = data + wk * α^i
				}
				syn[i] = wk;
				hasErr |= wk;
			}
			return hasErr == 0;
		}
	}

	public class Galois81D: Galois
	{
		public const int POLYNOMIAL = 0x1d;

		public static Galois81D instance = new Galois81D();

		Galois81D()
			: base(POLYNOMIAL, 8)
		{
		}
	}

	public class Galois16 : Galois
	{
		public const int POLYNOMIAL = 0x1100B;

		public static Galois16 instance = new Galois16();

		Galois16()
			: base(POLYNOMIAL, 16)
		{
		}
	}
}
