namespace Ansroled.Common.Helpers;

public static class DesignDataHelper
{
    public static string EditorContent => 
        """
        ---
        - name: Ensure dependencies are installed
          become: true
          package:
            name: 
              - curl
              - sudo
            state: present

        - name: Download get-docker.sh script
          become: true
          get_url:
            url: "https://get.docker.com"
            dest: "/tmp/get-docker.sh"
            mode: 'u+x'

        - name: Install Docker using get-docker.sh
          become: true
          command: sh /tmp/get-docker.sh
          args:
            creates: "/usr/bin/docker"  # Ensures this runs only if Docker is not installed

        - name: Ensure Docker service is running
          become: true
          systemd:
            name: docker
            state: started
            enabled: true

        - name: Add user {{ ansible_user_id }} to docker group
          become: true
          user:
            name: "{{ ansible_user_id }}"
            groups: docker
            append: true

        - name: Remove installation script
          become: true
          file:
            path: "/tmp/get-docker.sh"
            state: absent

        #- name: Create symbolic link to volumes
        #  become: true
        #  file:
        #    src: "/var/lib/docker/volumes"
        #    dest: "/home/{{ ansible_user_id }}/docker_volumes"
        #    state: link
        #    owner: "{{ ansible_user_id }}"
        #    group: "{{ ansible_user_id }}"
        """;
}