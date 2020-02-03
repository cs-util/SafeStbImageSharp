﻿using StbImageLib.Utility;
using System;
using System.IO;

namespace StbImageLib.Decoding
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
# endif
	class PsdDecoder : Decoder
	{
		private PsdDecoder(Stream stream) : base(stream)
		{
		}

		private int stbi__psd_decode_rle(FakePtr<byte> p, int pixelCount)
		{
			int count = 0;
			int nleft = 0;
			int len = 0;
			count = (int)(0);
			while ((nleft = (int)(pixelCount - count)) > (0))
			{
				len = (int)(stbi__get8());
				if ((len) == (128))
				{
				}
				else if ((len) < (128))
				{
					len++;
					if ((len) > (nleft))
						return (int)(0);
					count += (int)(len);
					while ((len) != 0)
					{
						p.Value = (byte)(stbi__get8());
						p += 4;
						len--;
					}
				}
				else if ((len) > (128))
				{
					byte val = 0;
					len = (int)(257 - len);
					if ((len) > (nleft))
						return (int)(0);
					val = (byte)(stbi__get8());
					count += (int)(len);
					while ((len) != 0)
					{
						p.Value = (byte)(val);
						p += 4;
						len--;
					}
				}
			}
			return (int)(1);
		}

		private ImageResult InternalDecode(ColorComponents? requiredComponents, int bpc)
		{
			int pixelCount = 0;
			int channelCount = 0;
			int compression = 0;
			int channel = 0;
			int i = 0;
			int bitdepth = 0;
			int w = 0;
			int h = 0;
			byte[] _out_;
			if (stbi__get32be() != 0x38425053)
				stbi__err("not PSD");
			if (stbi__get16be() != 1)
				stbi__err("wrong version");
			stbi__skip((int)(6));
			channelCount = (int)(stbi__get16be());
			if (((channelCount) < (0)) || ((channelCount) > (16)))
				stbi__err("wrong channel count");
			h = (int)(stbi__get32be());
			w = (int)(stbi__get32be());
			bitdepth = (int)(stbi__get16be());
			if ((bitdepth != 8) && (bitdepth != 16))
				stbi__err("unsupported bit depth");
			if (stbi__get16be() != 3)
				stbi__err("wrong color format");
			stbi__skip((int)(stbi__get32be()));
			stbi__skip((int)(stbi__get32be()));
			stbi__skip((int)(stbi__get32be()));
			compression = (int)(stbi__get16be());
			if ((compression) > (1))
				stbi__err("bad compression");

			var bits_per_channel = 8;
			if (((compression == 0) && ((bitdepth) == (16))) && ((bpc) == (16)))
			{
				_out_ = new byte[8 * w * h];
				bits_per_channel = (int)(16);
			}
			else
				_out_ = new byte[4 * w * h];
			pixelCount = (int)(w * h);

			FakePtr<byte> ptr = new FakePtr<byte>(_out_);
			if ((compression) != 0)
			{
				stbi__skip((int)(h * channelCount * 2));
				for (channel = (int)(0); (channel) < (4); channel++)
				{
					FakePtr<byte> p;
					p = ptr + channel;
					if ((channel) >= (channelCount))
					{
						for (i = (int)(0); (i) < (pixelCount); i++, p += 4)
						{
							p.Set((byte)((channel) == (3) ? 255 : 0));
						}
					}
					else
					{
						if (stbi__psd_decode_rle(p, (int)(pixelCount)) == 0)
						{
							stbi__err("corrupt");
						}
					}
				}
			}
			else
			{
				for (channel = (int)(0); (channel) < (4); channel++)
				{
					if ((channel) >= (channelCount))
					{
						if (((bitdepth) == (16)) && ((bpc) == (16)))
						{
							throw new NotImplementedException();
/*							ushort* q = ((ushort*)(ptr)) + channel;
							ushort val = (ushort)((channel) == (3) ? 65535 : 0);
							for (i = (int)(0); (i) < (pixelCount); i++, q += 4)
							{
								*q = (ushort)(val);
							}*/
						}
						else
						{
							FakePtr<byte> p = ptr + channel;
							byte val = (byte)((channel) == (3) ? 255 : 0);
							for (i = (int)(0); (i) < (pixelCount); i++, p += 4)
							{
								p.Set(val);
							}
						}
					}
					else
					{
						if ((bits_per_channel) == (16))
						{
							throw new NotImplementedException();
/*							ushort* q = ((ushort*)(ptr)) + channel;
							for (i = (int)(0); (i) < (pixelCount); i++, q += 4)
							{
								*q = ((ushort)(stbi__get16be()));
							}*/
						}
						else
						{
							FakePtr<byte> p = ptr + channel;
							if ((bitdepth) == (16))
							{
								for (i = (int)(0); (i) < (pixelCount); i++, p += 4)
								{
									p.Set(((byte)(stbi__get16be() >> 8)));
								}
							}
							else
							{
								for (i = (int)(0); (i) < (pixelCount); i++, p += 4)
								{
									p.Set((byte)(stbi__get8()));
								}
							}
						}
					}
				}
			}

			if ((channelCount) >= (4))
			{
				if ((bits_per_channel) == (16))
				{
					throw new NotImplementedException();

/*					for (i = (int)(0); (i) < (w * h); ++i)
					{
						ushort* pixel = (ushort*)(ptr) + 4 * i;
						if ((pixel[3] != 0) && (pixel[3] != 65535))
						{
							float a = (float)(pixel[3] / 65535.0f);
							float ra = (float)(1.0f / a);
							float inv_a = (float)(65535.0f * (1 - ra));
							pixel[0] = ((ushort)(pixel[0] * ra + inv_a));
							pixel[1] = ((ushort)(pixel[1] * ra + inv_a));
							pixel[2] = ((ushort)(pixel[2] * ra + inv_a));
						}
					}*/
				}
				else
				{
					for (i = (int)(0); (i) < (w * h); ++i)
					{
						FakePtr<byte> pixel = ptr + 4 * i;
						if ((pixel[3] != 0) && (pixel[3] != 255))
						{
							float a = (float)(pixel[3] / 255.0f);
							float ra = (float)(1.0f / a);
							float inv_a = (float)(255.0f * (1 - ra));
							pixel[0] = ((byte)(pixel[0] * ra + inv_a));
							pixel[1] = ((byte)(pixel[1] * ra + inv_a));
							pixel[2] = ((byte)(pixel[2] * ra + inv_a));
						}
					}
				}
			}

			var req_comp = requiredComponents.ToReqComp();
			if (((req_comp) != 0) && (req_comp != 4))
			{
				if ((bits_per_channel) == (16))
					_out_ = Conversion.stbi__convert_format16(_out_, (int)(4), (int)(req_comp), (uint)(w), (uint)(h));
				else
					_out_ = Conversion.stbi__convert_format(_out_, (int)(4), (int)(req_comp), (uint)(w), (uint)(h));
			}

			return new ImageResult
			{
				Width = w,
				Height = h,
				SourceComponents = ColorComponents.RedGreenBlueAlpha,
				ColorComponents = requiredComponents != null ? requiredComponents.Value : ColorComponents.RedGreenBlueAlpha,
				BitsPerChannel = bits_per_channel,
				Data = _out_
			};
		}

		public static bool Test(Stream stream)
		{
			var r = (((stream.stbi__get32be()) == (0x38425053)));
			stream.Rewind();

			return r;
		}

		public static ImageInfo? Info(Stream stream)
		{
			try
			{
				if (stream.stbi__get32be() != 0x38425053)
				{
					return null;
				}

				if (stream.stbi__get16be() != 1)
				{
					return null;
				}

				stream.stbi__skip(6);
				var channelCount = stream.stbi__get16be();
				if (((channelCount) < (0)) || ((channelCount) > (16)))
				{
					return null;
				}

				var height = (int)(stream.stbi__get32be());
				var width = (int)(stream.stbi__get32be());
				var depth = (int)(stream.stbi__get16be());
				if ((depth != 8) && (depth != 16))
				{
					return null;
				}

				if (stream.stbi__get16be() != 3)
				{
					return null;
				}

				return new ImageInfo
				{
					Width = width,
					Height = height,
					ColorComponents = ColorComponents.RedGreenBlueAlpha,
					BitsPerChannel = depth
				};
			}
			finally
			{
				stream.Rewind();
			}
		}

		public static ImageResult Decode(Stream stream, ColorComponents? requiredComponents = null, int bpc = 8)
		{
			var decoder = new PsdDecoder(stream);
			return decoder.InternalDecode(requiredComponents, bpc);
		}
	}
}