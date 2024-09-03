require 'securerandom'
require 'fileutils'

def rename_recursively(dir)
  Dir.foreach(dir) do |dir_or_file|
    next if dir_or_file == '.' || dir_or_file == '..' # Skip current and parent directory entries

    full_path = File.join(dir, dir_or_file)
    new_name = SecureRandom.uuid

    if File.directory?(full_path)
      rename_directory(dir, full_path, new_name)
    else
      rename_file(dir, full_path, new_name)
    end
  end
end

def rename_directory(dir, full_path, new_name)
  new_dir_path = File.join(dir, new_name)
  FileUtils.mv(full_path, new_dir_path)
  puts "Renamed directory '#{full_path}' to '#{new_dir_path}'"

  rename_recursively(new_dir_path)
end

def rename_file(dir, full_path, new_base_name)
  ext = File.extname(full_path) # Includes the period.

  unless is_valid_extension?(ext)
    puts "Ignoring #{full_path}"
    return
  end

  new_file_name = "#{new_base_name}#{ext}"
  new_file_path = File.join(dir, new_file_name)
  FileUtils.mv(full_path, new_file_path)
  puts "Renamed file '#{full_path}' to '#{new_file_path}'"
end

def is_valid_extension?(path)
  %w[.mp3 .m4a .mp4 .aac .ogg .opus .flac].include? path.downcase
end

if ARGV.length != 1
  puts "Usage: ruby recursive_random_rename.rb DIRECTORY_PATH"
  exit 1
end

directory = ARGV[0]

# Check if the directory exists
unless Dir.exist?(directory)
  puts "Error: Directory '#{directory}' does not exist."
  exit 1
end

rename_recursively(directory)
