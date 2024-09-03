require 'securerandom'
require 'fileutils'

def rename_recursively(dir, whitelisted_extensions)
  Dir.foreach(dir) do |dir_or_file|
    next if dir_or_file == '.' || dir_or_file == '..' # Skip current and parent directory entries

    full_path = File.join(dir, dir_or_file)
    new_name = SecureRandom.uuid

    if File.directory?(full_path)
      rename_directory(dir, full_path, new_name)
    else
      rename_file(dir, full_path, new_name, whitelisted_extensions)
    end
  end
end

def rename_directory(dir, full_path, new_name)
  new_dir_path = File.join(dir, new_name)
  FileUtils.mv(full_path, new_dir_path)
  puts "Renamed directory '#{full_path}' to '#{new_dir_path}'"

  rename_recursively(new_dir_path)
end

def rename_file(dir, full_path, new_base_name, whitelisted_extensions)
  ext = File.extname(full_path) # Includes the period.

  if whitelisted_extensions.any? && !whitelisted_extensions.include?(ext)
    puts "Ignoring #{full_path}"
    return
  end

  new_file_name = "#{new_base_name}#{ext}"
  new_file_path = File.join(dir, new_file_name)
  FileUtils.mv(full_path, new_file_path)
  puts "Renamed file '#{full_path}' to '#{new_file_path}'"
end

unless ARGV.length.between?(1, 2)
  puts "Usage: ruby recursive_random_rename.rb DIRECTORY_PATH"
  puts "          • Updates all files"
  puts
  puts "       ruby recursive_random_rename.rb DIRECTORY_PATH EXT1,EXT2,EXT3"
  puts "          • Renames only files with the specific extensions"
  puts "          • The initial period is optional (i.e., '.m4a' == 'm4a')"
  puts "          • Sample: ruby recursive_random_rename.rb . m4a,mp3,ogg"
  exit 1
end

directory = ARGV[0]
whitelisted_extensions = ARGV.length == 2 ? ARGV[1].split(",").map(&:strip).map { |i| i[0] == '.' ? i : ".#{i}" } : []

# Check if the directory exists
unless Dir.exist?(directory)
  puts "Error: Directory '#{directory}' does not exist."
  exit 1
end

rename_recursively(directory, whitelisted_extensions)
