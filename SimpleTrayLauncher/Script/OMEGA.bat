netsh wlan connect ssid="AGV-OMEGA" name="AGV-OMEGA"  interface="Wi-Fi - Interna"
netsh interface ip set address "Wi-Fi - Interna" static 10.25.91.2 255.255.254.0 10.25.90.1 1
