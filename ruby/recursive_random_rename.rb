require 'securerandom'
require 'fileutils'

def rename_directory_items(dir, whitelisted_extensions)
  results = { dirs: 0, files: { success: 0, ignored: 0 } }

  Dir.foreach(dir) do |dir_or_file|
    next if dir_or_file == '.' || dir_or_file == '..' || dir_or_file[0] == '.'

    full_path = File.join(dir, dir_or_file)
    new_name = SecureRandom.uuid

    if File.directory?(full_path)
      new_dir_path = rename_directory(dir, full_path, new_name)
      results[:dirs] += 1

      inner_results = rename_directory_items(new_dir_path, whitelisted_extensions)
      results = merge_hashes(results, inner_results)
    else
      rename_result_type = rename_file(dir, full_path, new_name, whitelisted_extensions)
      results[:files][rename_result_type] += 1
    end
  end

  results
end

def merge_hashes(target, source)
  source.keys.each do |key|
    case source[key]
    when Hash then
      target[key] = merge_hashes(target[key], source[key])
    else
      target[key] += source[key]
    end
  end

  target
end

def rename_directory(dir, full_path, new_name)
  new_dir_path = File.join(dir, new_name)
  FileUtils.mv(full_path, new_dir_path)
  puts "Renamed directory '#{full_path}' to '#{new_dir_path}'"
  new_dir_path
end

def rename_file(dir, full_path, new_base_name, whitelisted_extensions)
  ext = File.extname(full_path) # Includes the period.

  if whitelisted_extensions.any? && !whitelisted_extensions.include?(ext)
    puts "Ignoring #{full_path}"
    return :ignored
  end

  new_file_name = "#{new_base_name}#{ext}"
  new_file_path = File.join(dir, new_file_name)
  FileUtils.mv(full_path, new_file_path)
  puts "Renamed file '#{full_path}' to '#{new_file_path}'"
  return :success
end

def print_results(results)
  dir_label = results[:dirs] == 1 ? "1 directory" : "#{results[:dirs]} directories"

  file_label_maker = -> (count) { count == 1 ? "1 file" : "#{count} files" }
  file_success_label = file_label_maker.call(results[:files][:success])
  file_ignore_label = file_label_maker.call(results[:files][:ignored])

  puts "Renamed #{dir_label} and #{file_success_label}. Ignored #{file_ignore_label}."
end

unless ARGV.length.between?(1, 2)
  puts "Usage: ruby recursive_random_rename.rb <DIRECTORY_PATH>"
  puts "          • Updates all files"
  puts
  puts "       ruby recursive_random_rename.rb <DIRECTORY_PATH> <EXT1,EXT2,EXT3,...>"
  puts "          • Renames only files with the specific extensions"
  puts "          • The initial period is optional (i.e., '.m4a' == 'm4a')"
  puts "          • Sample: ruby recursive_random_rename.rb . m4a,mp3,ogg"
  exit 1
end

directory = ARGV[0]

unless Dir.exist?(directory)
  puts "Error: Directory '#{directory}' does not exist."
  exit 1
end

whitelisted_extensions =
  if ARGV.length == 2
    ARGV[1].split(",")
           .map(&:strip)
           .map { |ext| ext[0] == '.' ? ext : ".#{ext}" }
  else
    []
  end

results = rename_directory_items(directory, whitelisted_extensions)
print_results(results)
