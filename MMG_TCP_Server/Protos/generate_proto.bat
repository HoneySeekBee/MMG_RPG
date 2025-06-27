@echo off
set PROTO_PATH=.
set OUTPUT_PATH=.\Generated

echo [INFO] proto 경로: %PROTO_PATH%
echo [INFO] 출력 경로: %OUTPUT_PATH%

for %%F in (*.proto) do (
    echo [INFO] 변환 중: %%F
    protoc --proto_path="%PROTO_PATH%" --csharp_out="%OUTPUT_PATH%" "%%F"
)

echo [INFO] 변환 완료!
pause