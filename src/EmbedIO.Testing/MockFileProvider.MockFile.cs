namespace EmbedIO.Testing
{
    partial class MockFileProvider
    {
        private sealed class MockFile : MockDirectoryEntry
        {
            public MockFile(byte[] data)
            {
                Data = data ??[];
            }

            public MockFile(string text)
            {
                Data = text == null
                    ?[]
                    : WebServer.DefaultEncoding.GetBytes(text);
            }

            public byte[] Data { get; private set; }

            public void SetData(byte[] data)
            {
                Data = data ??[];
                Touch();
            }

            public void SetData(string text)
            {
                Data = text == null
                    ?[]
                    : WebServer.DefaultEncoding.GetBytes(text);
                Touch();
            }
        }
    }
}