:: Put ruby and its development kit in the system path
call "%~dp0\.dev\vars.cmd"

:: :: Don't forget the prerequisites
:: gem install bundle
:: bundle install

:: Start a local web server
bundle exec ruhoh server 6520
