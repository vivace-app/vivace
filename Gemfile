source "https://rubygems.org"

gem 'cocoapods'
gem "fastlane"
gem 'fastlane-plugin-github_action', git: "https://github.com/joshdholtz/fastlane-plugin-github_action" # The published gem is missing necessary changes, so we need to link directly to the git repo

plugins_path = File.join(File.dirname(__FILE__), 'Fastlane', 'Pluginfile')
eval_gemfile(plugins_path) if File.exist?(plugins_path)
