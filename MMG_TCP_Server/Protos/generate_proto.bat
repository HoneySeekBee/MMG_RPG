@echo off
set PROTO_PATH=.
set OUTPUT_PATH=.\Generated

echo [INFO] proto ���: %PROTO_PATH%
echo [INFO] ��� ���: %OUTPUT_PATH%

for %%F in (*.proto) do (
    echo [INFO] ��ȯ ��: %%F
    protoc --proto_path="%PROTO_PATH%" --csharp_out="%OUTPUT_PATH%" "%%F"
)

echo [INFO] ��ȯ �Ϸ�!
pause