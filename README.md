# Auto reset domain NoIp

Build your own image?
```bash
git clone https://github.com/kvmta/NoIP.git
cd NoIP/src
docker build -t cullen2205/noip .
```

Run in docker?
```bash
# override settings
setting='username=replace_with_your_username
password=replace_with_your_password
hosts=replace_with_your_hostname.ddns.net'
echo $setting > Settings/appsettings

# execute
docker run -v $pwd/Settings:/app/Settings --name noip -d cullen2205/noip:latest
```

Crontab
```bash
# schedule it in crontab, avoid expiration after 30 days
# 1st & 15th day every month
0 0 1,15 * * docker run cullen2205/noip:latest
```